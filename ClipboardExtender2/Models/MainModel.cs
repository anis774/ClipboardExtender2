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

        private ForegroundWindowManager foregroundWindowManager;    // フォアグラウンドウィンドウを監視、変更する
        private IDisposable history_CollectionChangedObserver;



        public Model()
        {
            this.LoadHistory();

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



        /// <summary>
        /// Windows固有の処理を含む初期化を行う
        /// </summary>
        /// <param name="windowHandle"></param>
        public void Initialize(IntPtr windowHandle)
        {
            // クリップボードの監視を開始
            this.ClipboardWatcher = new ClipboardWatcher();
            this.ClipboardWatcher.StartListenClipboard(windowHandle);

            // グローバルホットキーの監視を開始
            this.HotKey = new HotKey();
            this.HotKey.ListenHotKey(
                windowHandle,
                System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift,
                System.Windows.Input.Key.B);

            // フォアグラウンドウィンドウの監視を開始
            this.foregroundWindowManager = new ForegroundWindowManager(new[] { windowHandle });

            this.loadingExtension(extinsionPath, this.ExtensionItems);
        }



        /// <summary>
        /// 拡張を読み込んでコンテキストメニューのデータ構造を構築する
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="extensionTreeItems"></param>
        public void loadingExtension(string dirPath, ObservableSynchronizedCollection<ExtensionTreeItem> extensionTreeItems)
        {
            foreach (var fileSystemInfo in Directory.GetFileSystemEntries(dirPath))
            {
                if (Directory.Exists(fileSystemInfo))
                {
                    var newExtensionTreeItem = new ExtensionTreeItem();
                    newExtensionTreeItem.Name = Path.GetFileNameWithoutExtension(fileSystemInfo);
                    extensionTreeItems.Add(newExtensionTreeItem);
                    this.loadingExtension(fileSystemInfo, newExtensionTreeItem.Items);
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

        private ClipboardWatcher _ClipboardWatcher;
        public ClipboardWatcher ClipboardWatcher
        {
            get { return this._ClipboardWatcher; }
            set
            {
                if (null != this._ClipboardWatcher)
                {
                    this._ClipboardWatcher.DrawClipboard -= this.clipboardWatcher_DrawClipboard;
                    this._ClipboardWatcher.Dispose();
                }
                this._ClipboardWatcher = value;
                this._ClipboardWatcher.DrawClipboard += this.clipboardWatcher_DrawClipboard;
            }
        }



        private HotKey _HotKey;
        public HotKey HotKey
        {
            get { return this._HotKey; }
            set
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



        private string _SelectedHistoryItem;
        /// <summary>
        /// 選択されているクリップボードの履歴
        /// </summary>
        public string SelectedHistoryItem
        {
            get
            {
                return this._SelectedHistoryItem;
            }
            set
            {
                if (this._SelectedHistoryItem == value)
                {
                    return;
                }
                this._SelectedHistoryItem = value;
                base.RaisePropertyChanged(() => SelectedHistoryItem);
            }
        }



        /// <summary>
        /// 選択されているクリップボードの履歴(複数)
        /// 読取専用。変更しても表示に影響しない。
        /// </summary>
        public string[] SelectedHistoryItems { private get; set; }



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
            string text = this.SelectedHistoryItem;
            Clipboard.SetText(text);

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
            this.SelectedHistoryItem = args.Out.ToString();

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
        private void clipboardWatcher_DrawClipboard(object sender, EventArgs e)
        {
            var text = Clipboard.GetText();
            if (string.IsNullOrEmpty(text)) { return; }

            // 既に同じテキストがあれば先頭に移動する
            int index = this.History.IndexOf(text);
            if (-1 == index)
            {
                this.History.Insert(0, text);
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
            this.ClipboardWatcher.DrawClipboard -= this.clipboardWatcher_DrawClipboard;
            this.ClipboardWatcher.Dispose();
       
            this.history_CollectionChangedObserver.Dispose();
            
            this.HotKey.HotKeyPushed -= this.hotKey_HotKeyPush;
            this.HotKey.Dispose();
            
            this.SaveHistory();
        }
    }
}
