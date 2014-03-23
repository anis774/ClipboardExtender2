using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClipboardExtender2.Views;

namespace ClipboardExtender2Test.Views
{
    [TestClass]
    public class TextLineCountConverterTest
    {
        private TextLineCountConverter model;

        [TestInitialize]
        public void Initialize()
        {
            model = new TextLineCountConverter();
        }



        [TestMethod]
        [TestProperty("a", "b")]
        public void TestConvert()
        {
            var r = (int)this.model.Convert("hoge", null, null, null);
            Assert.AreEqual(1, r);

            r = (int)this.model.Convert("hoge\r\n", null, null, null);
            Assert.AreEqual(1, r);

            r = (int)this.model.Convert("hoge\r", null, null, null);
            Assert.AreEqual(1, r);

            r = (int)this.model.Convert("hoge\n", null, null, null);
            Assert.AreEqual(1, r);


            r = (int)this.model.Convert("hoge\r\nfuga", null, null, null);
            Assert.AreEqual(2, r);

            r = (int)this.model.Convert("hoge\rfuga", null, null, null);
            Assert.AreEqual(2, r);

            r = (int)this.model.Convert("hoge\nfuga", null, null, null);
            Assert.AreEqual(2, r);


            r = (int)this.model.Convert("hoge\r\nfuga\r\n", null, null, null);
            Assert.AreEqual(2, r);

            r = (int)this.model.Convert("hoge\rfuga\r", null, null, null);
            Assert.AreEqual(2, r);

            r = (int)this.model.Convert("hoge\nfuga\n", null, null, null);
            Assert.AreEqual(2, r);

            r = (int)this.model.Convert("hoge\r\n\r\nfuga", null, null, null);
            Assert.AreEqual(3, r);

            r = (int)this.model.Convert("hoge\r\rfuga", null, null, null);
            Assert.AreEqual(3, r);

            r = (int)this.model.Convert("hoge\n\nfuga", null, null, null);
            Assert.AreEqual(3, r);
        }


        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestConvertBack()
        {
            this.model.ConvertBack(null, null, null, null);
        }
    }
}
