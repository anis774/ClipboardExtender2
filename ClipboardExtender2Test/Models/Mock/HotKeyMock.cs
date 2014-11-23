using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;

using ClipboardExtender2.Models;

namespace ClipboardExtender2Test.Models.Mock
{
    public class HotKeyMock : IHotKey
    {
        public bool Disposed { get; set; }
        public void Dispose()
        {
            this.Disposed = true;
        }

        public event EventHandler HotKeyPushed;
        public void RaiseHotKeyPushed()
        {
            EventHandler h = this.HotKeyPushed;
            if (h != null)
            {
                h(this, EventArgs.Empty);
            }
        }

        public Tuple<IntPtr, ModifierKeys, Key> ListenHotKeyArgs = null;
        public bool ListenHotKey(IntPtr handle, ModifierKeys modKey, Key key)
        {
            this.ListenHotKeyArgs = new Tuple<IntPtr, ModifierKeys, Key>(handle, modKey, key);
            return true;
        }
    }
}
