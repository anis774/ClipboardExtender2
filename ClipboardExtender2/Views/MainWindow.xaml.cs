using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VM = ClipboardExtender2.ViewModels;
using Livet.Commands;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace ClipboardExtender2.Views
{
    /* 
     * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        CompositeDisposable rxDisposables = new CompositeDisposable();

        private VM.MainWindowViewModel _VM;
        private VM.MainWindowViewModel VM
        {
            get
            {
                if (this._VM == null)
                {
                    this._VM = this.DataContext as VM.MainWindowViewModel;
                }

                return this._VM;
            }
        }

        public MainWindow()
        {
            // グローバリゼーションテスト用
            // Properties.Resources.Culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            InitializeComponent();
            
            // ホットキーによってウィンドウがアクティブになったらリストボックスの先頭のアイテムにフォーカスを当てる。
            // 単にActivatedのタイミングで制御を行うとホットキーの修飾キー(Ctrl+Shift)が押されているためにおかしな動作をする
            // そこでホットキーによってアクティブになってから修飾キーが離されたタイミングで制御を行なう
            this.rxDisposables.Add(
                Observable.FromEventPattern<EventArgs>(this, "Activated")
                    .Where(_ => Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))     // Ctrl、Shiftが押されていればホットキーによるものとする
                    .Select(_ => _.EventArgs)
                    .Merge(Observable.FromEventPattern<KeyEventArgs>(this, "KeyUp")
                        .Where(_ => new[] { Key.LeftCtrl, Key.LeftShift, Key.RightCtrl, Key.RightShift }.Contains(_.EventArgs.Key)
                            && !Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))    // CtrlとShiftのKeyUpは別々に上がってくるので最後に上がってきたイベントのみを通す
                        .Select(_ => _.EventArgs))
                    .Scan((before, after) => before != null && !(before is KeyEventArgs) && after is KeyEventArgs ? null : after)       // Activated→KeyUpの順で来た場合のみ通す(Activeted→KeyUp→KeyUpと来ても2回目のKeyUpは通さない)
                    .Where(_ => _ == null)
                    .Subscribe(_ => {
                        this.mainListBox.ScrollIntoView(this.VM.ClipbordHistory.FirstOrDefault());

                        this.mainListBox.Focus();
                        var item = (ListBoxItem)(this.mainListBox.ItemContainerGenerator.ContainerFromItem(this.VM.ClipbordHistory.FirstOrDefault()));
                        if (item != null)
                        {
                            item.Focus();
                        }
                    })
            );
        }


        public void ExtensionLoaded()
        {
            var extensionItems = this.VM.ExtensionItems;
            this.extensionContextMenu.Items.Clear();
            this.AddExtensionMenuItem(this.extensionContextMenu.Items, extensionItems);
        }



        private bool AddExtensionMenuItem(ItemCollection contextMenuItems, IEnumerable<ClipboardExtender2.Models.ExtensionTreeItem> extensionItems)
        {
            bool r = false;

            foreach (var item in extensionItems)
            {
                r = true;

                var menuItem = new MenuItem(){
                    Header = item.Name,
                };

                contextMenuItems.Add(menuItem);

                if (!this.AddExtensionMenuItem(menuItem.Items, item.Items))
                {
                    menuItem.Command = new ViewModelCommand(() =>
                    {
                        this.VM.ExtensionPaste(item.Extension);
                    });
                }
            }

            return r;
        }

        private void mainWindow_Activated(object sender, EventArgs e)
        {

        }

        private void mainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.VM.SelectedHistoryItems = this.mainListBox.SelectedItems.Cast<string>().ToArray();
        }

        private void mainWindow_Closed(object sender, EventArgs e)
        {
            this.rxDisposables.Dispose();
        }
    }
}
