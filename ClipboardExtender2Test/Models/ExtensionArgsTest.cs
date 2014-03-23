using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClipboardExtender;

namespace ClipboardExtender2Test.Models
{
    [TestClass]
    public class ExtensionArgsTest
    {
        private ExtensionArgs extensionArgs;

        [TestInitialize]
        public void Initialize()
        {
            this.extensionArgs = new ExtensionArgs();
        }

        [TestMethod]
        public void TestValues()
        {
            this.extensionArgs.Values = new[] {
                "hoge",
                "fuga\npiyo",
                "foo\rbar",
                "hogefuga\r\nfugapiyo",
                "hoge\r\rfuga",
                "fuga\n\nhoge",
                "piyo\r\n\r\nfoo"
            };

            CollectionAssert.AreEqual(
                new[]{
                    "hoge",
                    "fuga",
                    "piyo",
                    "foo",
                    "bar",
                    "hogefuga",
                    "fugapiyo",
                    "hoge",
                    "",
                    "fuga",
                    "fuga",
                    "",
                    "hoge",
                    "piyo",
                    "",
                    "foo"
                },
                this.extensionArgs.Lines);
        }
    }
}
