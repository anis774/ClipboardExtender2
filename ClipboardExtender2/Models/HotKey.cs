using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace ClipboardExtender2.Models
{
    /// <summary>
    /// グローバルホットキーを登録するクラス。
    /// 使用後は必ずDisposeすること。
    /// </summary>
    public class HotKey: IDisposable, IHotKey
    {
        private HwndSource hwndSource = null;
        private int id;

        /// <summary>
        /// ホットキーの監視を開始する
        /// </summary>
        /// <param name="handle">登録に使うウィンドウのハンドル</param>
        /// <param name="modKey">登録する装飾キー</param>
        /// <param name="key">登録するキー</param>
        /// <returns>登録の成否</returns>
        public bool ListenHotKey(IntPtr handle, ModifierKeys modKey, Key key)
        {
            this.hwndSource = HwndSource.FromHwnd(handle);
            this.hwndSource.AddHook(this.WndProc);

            for (int i = 0x0000; i <= 0xbfff; i++)
            {
                if (RegisterHotKey(this.hwndSource.Handle, i, modKey, KeyInterop.VirtualKeyFromKey(key)) != 0)
                {
                    this.id = i;
                    return true;
                }
            }
            return false;
        }

        [DllImport("user32.dll")]
        extern static int RegisterHotKey(IntPtr HWnd, int ID, ModifierKeys MOD_KEY, int KEY);

        [DllImport("user32.dll")]
        extern static int UnregisterHotKey(IntPtr HWnd, int ID);

        const int WM_HOTKEY = 0x0312;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && (int)wParam == this.id)
            {
                this.RaiseHotKeyPushed();
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void RaiseHotKeyPushed()
        {
            var h = this.HotKeyPushed;
            if (null != h)
            {
                h(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// ホットキーが押されると発生する。
        /// </summary>
        public event EventHandler HotKeyPushed;

        public void Dispose()
        {
            UnregisterHotKey(this.hwndSource.Handle, this.id);
            this.hwndSource.Dispose();
        }
    }
}
