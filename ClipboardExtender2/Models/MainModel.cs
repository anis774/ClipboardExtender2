using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reactive.Linq;
using ClipboardExtender;

using Livet;

namespace ClipboardExtender2.Models
{
    public class Model : NotificationObject, IDisposable
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        private const string historyFilePath = ".\\history.dat";    // クリップボードの履歴ファイルのパス
        private const string extinsionPath = ".\\extension";        // 拡張を配置するパス

        private IForegroundWindowManager foregroundWindowManager;    // フォアグラウンドウィンドウを監視、変更する
        private IDisposable history_CollectionChangedObserver;



        private Model()
        {
            // ClipboardExtender2をオンラインストレージに置いて使っている場合、
            // PCのシャットダウンでClipboardExtender2が終了される際に保存をするだけでは
            // オンラインストレージとの同期が終わらないうちにシャットダウンされデータの一部が消失することがある。
            // かといって内容が変わる度に保存をしては負荷がかかるので5秒のウェイトを置いて定期保存する
            this.history_CollectionChangedObserver =
                Observable.FromEventPattern(this.History, "CollectionChanged")
                .Throttle(TimeSpan.FromSeconds(5))
                .Subscribe(
                    _ => this.SaveHistory()
                );
        }



        private static Model _Instance;
        public static Model Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Model();
                }

                return _Instance;
            }
        }



        /// <summary>
        /// テスト用
        /// </summary>
        public static void ClearInstance()
        {
            if (_Instance != null)
            {
                _Instance.Dispose();
            }
            _Instance = null;
        }



        /// <summary>
        /// Windows固有の処理を含む初期化を行う
        /// </summary>
        /// <param name="windowHandle"></param>
        public void Initialize(IntPtr windowHandle)
        {
            this.Initialize(
                windowHandle,
                new ClipboardListener(windowHandle),
                new HotKey(),
                new ForegroundWindowManager(new[] { windowHandle })
            );

            this.LoadHistory();
        }



        public void Initialize(IntPtr windowHandle, IClipboardListener clipboardListener, IHotKey hotKey, IForegroundWindowManager foregroundWindowManager)
        {
             // クリップボードの監視を開始
            if (clipboardListener != null)
            {
                this.ClipboardListener = clipboardListener;
                this.ClipboardListener.Start();
            }

            // グローバルホットキーの監視を開始
            if (hotKey != null)
            {
                this.HotKey = hotKey;
                this.HotKey.ListenHotKey(
                    windowHandle,
                    System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift,
                    System.Windows.Input.Key.B);
            }

            // フォアグラウンドウィンドウの監視を開始
            if (foregroundWindowManager != null)
            {
                this.foregroundWindowManager = foregroundWindowManager;
            }

            this.LoadingExtension(extinsionPath, this.ExtensionItems);           
        }



        /// <summary>
        /// 拡張を読み込んでコンテキストメニューのデータ構造を構築する
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="extensionTreeItems"></param>
        public void LoadingExtension(string dirPath, ObservableSynchronizedCollection<ExtensionTreeItem> extensionTreeItems)
        {
            foreach (var fileSystemInfo in Directory.GetFileSystemEntries(dirPath))
            {
                if (Directory.Exists(fileSystemInfo))
                {
                    var newExtensionTreeItem = new ExtensionTreeItem();
                    newExtensionTreeItem.Name = Path.GetFileNameWithoutExtension(fileSystemInfo);
                    extensionTreeItems.Add(newExtensionTreeItem);
                    this.LoadingExtension(fileSystemInfo, newExtensionTreeItem.Items);
                }
                else if (".txt" != Path.GetExtension(fileSystemInfo))
                {
                    var newExtensionTreeItem = new ExtensionTreeItem(fileSystemInfo);
                    newExtensionTreeItem.Name = Path.GetFileNameWithoutExtension(fileSystemInfo);
                    extensionTreeItems.Add(newExtensionTreeItem);
                }
            }

            var h = this.ExtensionLoaded;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        #region プロパティ

        private IClipboardListener _ClipboardListener;
        public IClipboardListener ClipboardListener
        {
            get { return this._ClipboardListener; }
            private set
            {
                if (null != this._ClipboardListener)
                {
                    this._ClipboardListener.ClipboardChanged -= this.ClipboardListener_DrawClipboard;
                    this._ClipboardListener.Dispose();
                }
                this._ClipboardListener = value;
                this._ClipboardListener.ClipboardChanged += this.ClipboardListener_DrawClipboard;
            }
        }



        private IHotKey _HotKey;
        public IHotKey HotKey
        {
            get { return this._HotKey; }
            private set
            {
                if (null != this._HotKey)
                {
                    this._HotKey.HotKeyPushed -= this.hotKey_HotKeyPush;
                    this._HotKey.Dispose();
                }
                this._HotKey = value;
                this._HotKey.HotKeyPushed += this.hotKey_HotKeyPush;
            }
        }



        private ObservableSynchronizedCollection<string> _History = new ObservableSynchronizedCollection<string>();
        /// <summary>
        /// クリップボードの履歴
        /// </summary>
        public ObservableSynchronizedCollection<string> History
        {
            get
            {
                return this._History;
            }
            private set
            {
                this._History = value;
            }
        }



        private string[] _SelectedHistoryItems = new string[] { };
        /// <summary>
        /// 選択されているクリップボードの履歴(複数)
        /// 読取専用。変更しても表示に影響しない。
        /// </summary>
        public string[] SelectedHistoryItems
        {
            private get
            {
                return this._SelectedHistoryItems;
            }
            set
            {
                this._SelectedHistoryItems = value;
            }
        }



        private ObservableSynchronizedCollection<ExtensionTreeItem> _ExtensionItems = new ObservableSynchronizedCollection<ExtensionTreeItem>();
        /// <summary>
        /// 読み込まれた拡張をツリー構造で保持
        /// </summary>
        public ObservableSynchronizedCollection<ExtensionTreeItem> ExtensionItems
        {
            get { return this._ExtensionItems; }
        }

        #endregion



        #region publicメソッド
        
        public void Paste()
        {
            string text = this.SelectedHistoryItems.FirstOrDefault();
            Clipboard.SetText(text == null ? string.Empty : text);

            this.foregroundWindowManager.RollbackForegroundWindow();
            System.Windows.Forms.SendKeys.SendWait("^v");

            var h = this.WindowClose;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }



        public void ExtensionPaste(IExtension extension)
        {
            ExtensionArgs args = new ExtensionArgs()
            {
                Values = this.SelectedHistoryItems
            };

            extension.Run(args);
            this.History.Insert(0, args.Out.ToString());

            if (!args.IsPasteCancel)
            {
                this.Paste();
            }
        }
        
        #endregion



        #region privateメソッド
        
        /// <summary>
        /// クリップボードの履歴をファイルから復元する
        /// </summary>
        private void LoadHistory()
        {
            var deserializer = new BinaryFormatter();
            try
            {
                using (var fileStream = new FileStream(Model.historyFilePath, FileMode.Open, FileAccess.Read))
                {
                    var history = (List<string>)deserializer.Deserialize(fileStream);
                    this.History = new ObservableSynchronizedCollection<string>(history);
                }
            }
            catch (FileNotFoundException)
            {
                // ファイルが無いのは想定内なので何もしない
            }
        }



        /// <summary>
        /// クリップボードの履歴をファイルに保存する
        /// </summary>
        private void SaveHistory()
        {
            var serializer = new BinaryFormatter();
            using (var fileStream = new FileStream(Model.historyFilePath, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(fileStream, this.History.ToList());
            }
        }

        #endregion



        #region イベントハンドラ
        /// <summary>
        /// グローバルホットキーが押されると発火する
        /// </summary>
        private void hotKey_HotKeyPush(object sender, EventArgs e)
        {
            var h = this.HotKeyPushed;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }



        /// <summary>
        /// クリップボードの内容が変更されると発火する
        /// </summary>
        private void ClipboardListener_DrawClipboard(object sender, ClipboardChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text)) { return; }

            // 既に同じテキストがあれば先頭に移動する
            int index = this.History.IndexOf(e.Text);
            if (-1 == index)
            {
                this.History.Insert(0, e.Text);
            }
            else
            {
                this.History.Move(index, 0);
            }
        }

        #endregion



        #region イベント

        public event EventHandler HotKeyPushed;
        public event EventHandler WindowClose;
        public event EventHandler ExtensionLoaded;
        
        #endregion



        /// <summary>
        /// アプリケーション終了時に呼ばれる
        /// </summary>
        public void Dispose()
        {
            if (this.ClipboardListener != null)
            {
                this.ClipboardListener.ClipboardChanged -= this.ClipboardListener_DrawClipboard;
                this.ClipboardListener.Dispose();
            }
       
            this.history_CollectionChangedObserver.Dispose();

            if (this.HotKey != null)
            {
                this.HotKey.HotKeyPushed -= this.hotKey_HotKeyPush;
                this.HotKey.Dispose();
            }

            if (this.foregroundWindowManager != null)
            {
                this.foregroundWindowManager.Dispose();
            }
            
            this.SaveHistory();
        }
    }
}
