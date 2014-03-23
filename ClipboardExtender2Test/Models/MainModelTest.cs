using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClipboardExtender2.Models;
using System.Collections.Generic;
using System.Reflection;

namespace ClipboardExtender2Test.Models
{
    [TestClass]
    public class MainModelTest
    {
        Model model;
        List<string> propertyChangedHistory = new List<string>();



        [TestInitialize]
        public void Initialize()
        {
            this.model = new Model();

            var propChangedHistory = new List<string>();
            this.model.PropertyChanged += model_PropertyChanged;
        }



        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.propertyChangedHistory.Add(e.PropertyName);
        }



        [TestMethod]
        public void TestHistory()
        {
            Assert.IsInstanceOfType(this.model.History, typeof(Livet.ObservableSynchronizedCollection<string>),
                "HistoryはLivet.ObservableSynchronizedCollection<string>型で初期化される");
        }



        [TestMethod]
        public void TestSeletedHistoryItem()
        {
            Assert.AreEqual(null, this.model.SelectedHistoryItem);

            this.model.SelectedHistoryItem = "hoge";
            Assert.AreEqual("hoge", this.model.SelectedHistoryItem);
            CollectionAssert.AreEqual(new[] { "SelectedHistoryItem" }, this.propertyChangedHistory,
                "SelectedHistoryItemに値がセットされるとSelectedHistoryItemのPropertyChangedが発火する");

            this.model.SelectedHistoryItem = "hoge";
            CollectionAssert.AreEqual(new[] { "SelectedHistoryItem" }, this.propertyChangedHistory,
                "同じ値がセットされてもPropertyChangedは発火しない");
        }



        [TestMethod]
        public void TestHotKeyPushed()
        {
            bool raised = false;
            var hotKey = new HotKey();
            this.model.HotKey = hotKey;
            this.model.HotKeyPushed += (s, e) => raised = true;

            hotKey.GetType()
                .GetMethod("RaiseHotKeyPushed", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(hotKey, new object[0]);

            Assert.IsTrue(raised,
                "ホットキーが押されるとHotKeyPushedイベントが発火する");
        }



        [TestMethod]
        public void TestclipboardWatcher_DrawClipboard()
        {
            var clipboardWatcher = new ClipboardWatcher();
            this.model.ClipboardWatcher = clipboardWatcher;
            MethodInfo raiseEventMethodInfo = clipboardWatcher.GetType()
                    .GetMethod("RaiseDrawClipboard", BindingFlags.NonPublic | BindingFlags.Instance);

            System.Windows.Clipboard.SetText("hoge");
            raiseEventMethodInfo.Invoke(clipboardWatcher, new object[0]);
            CollectionAssert.AreEqual(new[] { "hoge" }, this.model.History,
                "クリップボードの中味が変更されるとHistoryに追加される");

            System.Windows.Clipboard.SetText("fuga");
            raiseEventMethodInfo.Invoke(clipboardWatcher, new object[0]);
            CollectionAssert.AreEqual(new[] { "fuga", "hoge" }, this.model.History,
                "クリップボードの中味が変更されるとHistoryの先頭に追加される");

            System.Windows.Clipboard.SetText("hoge");
            raiseEventMethodInfo.Invoke(clipboardWatcher, new object[0]);
            CollectionAssert.AreEqual(new[] { "hoge", "fuga" }, this.model.History,
                "既にHistoryに含まれている内容にクリップボードの中味が変更されたら、そのアイテムを先頭に移動する");
        }
    }
}
