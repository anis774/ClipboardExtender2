using System;
using System.Collections.Generic;
//using System.Windows.Forms;

namespace ClipboardExtender
{
    public class ExtensionArgs
    {
        object out_ = "";

        public string[] Values { set; get; }

        public object Out
        {
            set
            {
                this.out_ = value;
            }
            get
            {
                return this.out_;
            }
        }

        public string Version
        {
            get
            {
                return string.Empty; //Application.ProductVersion;
            }
        }

        public bool IsPasteCancel { set; get; }

        public override string ToString()
        {
            if (this.Out != null)
            {
                return this.Out.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public string[] Lines
        {
            get
            {
                List<string> lines = new List<string>();

                foreach (string value in this.Values)
                {
                    foreach (string line in value.TrimEnd(new char[] { '\r', '\n' }).Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                    {
                        lines.Add(line);
                    }
                }

                return lines.ToArray();
            }
        }

        public void ChangeProgress(int max, int value)
        {
            //Form1.MainForm.SetProgress(max, value);
        }

        public void ChangeStatusText(string statusText)
        {
            //Form1.MainForm.SetStatusText(statusText, string.Empty);
        }
    }
}
