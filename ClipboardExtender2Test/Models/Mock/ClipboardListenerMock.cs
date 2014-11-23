using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClipboardExtender2.Models;

namespace ClipboardExtender2Test.Models.Mock
{
    public class ClipboardListenerMock: IClipboardListener
    {
        public bool Stared { get; set; }
        public bool Disposed { get; set; }
        
        public void Start()
        {
            this.Stared = true;
        }

        public event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;
        public void RaiseClipboardChanged(string text)
        {
            EventHandler<ClipboardChangedEventArgs> h = this.ClipboardChanged;
            if (h != null)
            {
                h(this, new ClipboardChangedEventArgs(text));
            }
        }

        public void Dispose()
        {
            this.Disposed = true;
        }
    }
}
