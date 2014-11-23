using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClipboardExtender2.Models
{
    class ClipboardListener: IClipboardListener
    {
        public event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;

        private ClipboardWatcher clipboardWatcher;
        private IntPtr windowHandle;

        public ClipboardListener(IntPtr windowHandle)
        {
            this.windowHandle = windowHandle;
            this.clipboardWatcher = new ClipboardWatcher();
            this.clipboardWatcher.DrawClipboard += this.ClipboardWatcher_DrawClipboard;
        }
        
        public void Start(IntPtr windowHandle)
        {
            this.clipboardWatcher.StartListenClipboard(this.windowHandle);
        }

        private void ClipboardWatcher_DrawClipboard(object sender, EventArgs e)
        {
            EventHandler<ClipboardChangedEventArgs> h = this.ClipboardChanged;
            if (h != null)
            {
                h(this, new ClipboardChangedEventArgs(Clipboard.GetText()));
            }
        }

        public void Dispose()
        {
            this.clipboardWatcher.DrawClipboard -= this.ClipboardWatcher_DrawClipboard;
            this.clipboardWatcher.Dispose();
        }

        public void Start()
        {
            this.clipboardWatcher.StartListenClipboard(this.windowHandle);
        }

    }
}
