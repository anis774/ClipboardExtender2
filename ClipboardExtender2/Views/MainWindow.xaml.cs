﻿using System;
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
        public MainWindow()
        {
            // グローバリゼーションテスト用
            // Properties.Resources.Culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            InitializeComponent();
        }



        public void ExtensionLoaded()
        {
            var extensionItems = ((VM.MainWindowViewModel)this.DataContext).ExtensionItems;
            this.extensionContextMenu.Items.Clear();
            this.AddExtensionMenuItem(this.extensionContextMenu.Items, extensionItems);
        }



        private void AddExtensionMenuItem(ItemCollection contextMenuItems, IEnumerable<ClipboardExtender2.Models.ExtensionTreeItem> extensionItems)
        {
            foreach (var item in extensionItems)
            {
                var menuItem = new MenuItem(){
                    Header = item.Name,
                };

                contextMenuItems.Add(menuItem);

                this.AddExtensionMenuItem(menuItem.Items, item.Items);
            }
        }
    }
}
