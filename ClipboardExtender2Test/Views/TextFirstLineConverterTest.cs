using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClipboardExtender2.Views;

namespace ClipboardExtender2Test.Views
{
    [TestClass]
    public class TextFirstLineConverterTest
    {
        private TextFirstLineConverter model;

        [TestInitialize]
        public void Initialize()
        {
            this.model = new TextFirstLineConverter();
        }



        [TestMethod]
        public void TestConvert()
        {
            object value = this.model.Convert("hoge" + Environment.NewLine + "fuga" + Environment.NewLine + "piyo", typeof(string), null, null);
            Assert.AreEqual("hoge", value);

            value = this.model.Convert("" + Environment.NewLine + "", typeof(string), null, null);
            Assert.IsNull(value);
        }



        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestConvertBack()
        {
            this.model.ConvertBack(null, null, null, null);
        }
    }
}
