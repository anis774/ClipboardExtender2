using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using ClipboardExtender2.Models;

namespace ClipboardExtender2.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        private Model model;

        public void Initialize()
        {
            this.model = new Model();
            this.ClipbordHistory = new DispatcherCollection<string>(this.model.History, DispatcherHelper.UIDispatcher);
            this.ExtensionItems = new DispatcherCollection<ExtensionTreeItem>(this.model.ExtensionItems, DispatcherHelper.UIDispatcher);

            this.CompositeDisposable.Add(
                new EventListener<EventHandler>(
                    _ => this.model.HotKeyPushed += _,
                    _ => this.model.HotKeyPushed -= _,
                    this.model_HotKeyPushed));

            this.CompositeDisposable.Add(
                new EventListener<EventHandler>(
                    _ => this.model.WindowClose += _,
                    _ => this.model.WindowClose -= _,
                    this.model_WindowClose));

            this.model.Initialize(new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow).Handle);
        }

        #region ClipbordHistory変更通知プロパティ
        private DispatcherCollection<string> _ClipbordHistory;

        public DispatcherCollection<string> ClipbordHistory
        {
            get
            { return _ClipbordHistory; }
            private set
            { 
                if (_ClipbordHistory == value)
                    return;
                _ClipbordHistory = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region CanClose変更通知プロパティ
        private bool _CanClose;

        public bool CanClose
        {
            get
            { return _CanClose; }
            set
            { 
                if (_CanClose == value)
                    return;
                _CanClose = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region SeletedHistoryItem変更通知プロパティ
        public string SeletedHistoryItem
        {
            get
            {
                if (this.model == null) return string.Empty;
                return this.model.SelectedHistoryItem;
            }
            set
            { 
                if (this.model == null || this.model.SelectedHistoryItem == value)
                    return;
                this.model.SelectedHistoryItem = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region ExtensionItems変更通知プロパティ
        private DispatcherCollection<ExtensionTreeItem> _ExtensionItems;
        public DispatcherCollection<ExtensionTreeItem> ExtensionItems
        {
            get
            { return _ExtensionItems; }
            set
            { 
                if (_ExtensionItems == value)
                    return;
                _ExtensionItems = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region ApplicationExitCommand
        private ViewModelCommand _ApplicationExitCommand;

        public ViewModelCommand ApplicationExitCommand
        {
            get
            {
                if (_ApplicationExitCommand == null)
                {
                    _ApplicationExitCommand = new ViewModelCommand(ApplicationExit);
                }
                return _ApplicationExitCommand;
            }
        }

        public void ApplicationExit()
        {
            this.CanClose = true;
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "close"));
        }
        #endregion


        #region MinimizeWindowCommand
        private ViewModelCommand _MinimizeWindowCommand;

        public ViewModelCommand MinimizeWindowCommand
        {
            get
            {
                if (_MinimizeWindowCommand == null)
                {
                    _MinimizeWindowCommand = new ViewModelCommand(MinimizeWindow);
                }
                return _MinimizeWindowCommand;
            }
        }

        public void MinimizeWindow()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Minimize, "minimize"));
        }
        #endregion


        #region PasteCommand
        private ViewModelCommand _PasteCommand;

        public ViewModelCommand PasteCommand
        {
            get
            {
                if (_PasteCommand == null)
                {
                    _PasteCommand = new ViewModelCommand(Paste);
                }
                return _PasteCommand;
            }
        }

        public void Paste()
        {
            this.model.Paste();
        }
        #endregion


        private void model_HotKeyPushed(object sender, EventArgs e)
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Normal, "normal"));
            Messenger.Raise(new WindowActionMessage(WindowAction.Active, "active"));
        }

        public void model_WindowClose(object sender, EventArgs e)
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Minimize, "minimize"));
        }

        protected override void Dispose(bool disposing)
        {
            this.model.Dispose();
            base.Dispose(disposing);
        }
    }
}
