using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClipboardExtender2.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;

using ClipboardExtender2Test.Models.Mock;

namespace ClipboardExtender2Test.Models
{
    [TestClass]
    public class MainModelTest
    {
        Model model;
        List<string> propertyChangedHistory = new List<string>();

        IntPtr windowHandle;
        ClipboardListenerMock clipboardListner;
        HotKeyMock hotKey;
        ForegroundWindowManagerMock foregroundWindowManager;


        [TestInitialize]
        public void Initialize()
        {
            Model.ClearInstance();
            this.model = Model.Instance;
            this.windowHandle = new IntPtr(12345);
            this.clipboardListner = new ClipboardListenerMock();
            this.hotKey = new HotKeyMock();
            this.foregroundWindowManager = new ForegroundWindowManagerMock();

            this.model.Initialize(
                this.windowHandle,
                this.clipboardListner,
                this.hotKey,
                this.foregroundWindowManager
            );

            this.model.PropertyChanged += (sender, e) => {
                this.propertyChangedHistory.Add(e.PropertyName);
            };
        }



        [TestCleanup]
        public void Cleanup()
        {
            this.model.Dispose();
            this.windowHandle = new IntPtr(12345);
            this.clipboardListner = new ClipboardListenerMock();
            this.hotKey = new HotKeyMock();
            this.foregroundWindowManager = new ForegroundWindowManagerMock();
        }



        [TestMethod]
        public void BeforeCreateInstance()
        {
            this.model.Dispose();

            Model.ClearInstance();
            this.model = Model.Instance;

            Assert.IsNull(this.model.ClipboardListener);
            Assert.IsNull(this.model.HotKey);
            Assert.IsInstanceOfType(this.model.History, typeof(Livet.ObservableSynchronizedCollection<string>));
            Assert.IsInstanceOfType(this.model.ExtensionItems, typeof(Livet.ObservableSynchronizedCollection<ExtensionTreeItem>));
        }


        [TestMethod]
        public void TestInitialize()
        {
            Assert.AreSame(this.clipboardListner, this.model.ClipboardListener);
            Assert.IsTrue(this.clipboardListner.Stared);

            Assert.AreSame(this.hotKey, this.model.HotKey);
            Assert.AreEqual(Tuple.Create(this.windowHandle, ModifierKeys.Control | ModifierKeys.Shift, Key.B), this.hotKey.ListenHotKeyArgs);
        }
        


        [TestMethod]
        public void TestOnHotKeyPushed()
        {
            var raised = false;

            this.model.HotKeyPushed += (sender, e) =>
            {
                raised = true;
            };

            this.hotKey.RaiseHotKeyPushed();
            Assert.IsTrue(raised);
        }



        [TestMethod]
        public void TestOnClipboardChanged()
        {
            this.clipboardListner.RaiseClipboardChanged("hoge");
            CollectionAssert.AreEqual(new[] { "hoge" }, this.model.History);

            this.clipboardListner.RaiseClipboardChanged("fuga");
            CollectionAssert.AreEqual(new[] { "fuga", "hoge" }, this.model.History);

            this.clipboardListner.RaiseClipboardChanged("hoge");
            CollectionAssert.AreEqual(new[] { "hoge", "fuga" }, this.model.History);
        }

        
        
        [TestMethod]
        public void TestDispose()
        {
            Assert.IsFalse(this.clipboardListner.Disposed);
            Assert.IsFalse(this.hotKey.Disposed);
            Assert.IsFalse(this.foregroundWindowManager.Disposed);

            this.model.Dispose();

            Assert.IsTrue(this.clipboardListner.Disposed);
            Assert.IsTrue(this.hotKey.Disposed);
            Assert.IsTrue(this.foregroundWindowManager.Disposed);
        }
    }
}
