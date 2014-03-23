using System;
using System.IO;
using System.Text;
//using System.Windows.Forms;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace ClipboardExtender
{
    class CbxExtension : IExtension
    {
        static ScriptEngine engine = Python.CreateEngine();
        ScriptSource source = null;
        string currentDirectory;

        public void Initialize(string path)
        {
            this.source = engine.CreateScriptSourceFromFile(path);
            this.currentDirectory = Path.GetDirectoryName(path);
        }

        public void Run(ExtensionArgs args)
        {
            lock (engine)
            {
                engine.SetSearchPaths(new[] { this.currentDirectory });
                ScriptScope scope = engine.CreateScope();

                scope.SetVariable("cbex", args);
                try
                {
                    source.Execute(scope);
                }
                catch (Exception ex)
                {
                    if (true) //MessageBox.Show("エラーの内容をクリップボードにコピーします。", "エラー - ClipboardExtender" + source.Path, MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                    {
                        StringBuilder message = new StringBuilder();
                        int indent = 0;

                        do
                        {
                            string indendText = new string('\t', indent++);
                            message.AppendLine(indendText + ex.ToString().Replace(Environment.NewLine, Environment.NewLine + indendText));
                        } while ((ex = ex.InnerException) != null);

                        //Form1.MainForm.Invoke((Action<string>)Clipboard.SetText, message.ToString());
                    }
                }
            }
        }
    }
}