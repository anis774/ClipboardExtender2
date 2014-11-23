using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardExtender2.Models
{
    public class ClipboardChangedEventArgs: EventArgs
    {
        public ClipboardChangedEventArgs(string text)
        {
            this.Text = text;
        }

        public string Text { get; private set; }
    }
}
