using System;
namespace ClipboardExtender2.Models
{
    public interface IHotKey
    {
        void Dispose();
        event EventHandler HotKeyPushed;
        bool ListenHotKey(IntPtr handle, System.Windows.Input.ModifierKeys modKey, System.Windows.Input.Key key);
    }
}
