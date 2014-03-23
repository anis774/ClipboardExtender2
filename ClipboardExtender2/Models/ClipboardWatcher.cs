using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

/// <summary>
/// クリップボードを監視するクラス。
/// 使用後は必ずDispose()メソッドを呼び出して下さい。
/// </summary>
public class ClipboardWatcher : IDisposable
{
    [DllImport("user32.dll")]
    private static extern IntPtr SetClipboardViewer(IntPtr hwnd);
    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")]
    private static extern bool ChangeClipboardChain(IntPtr hwnd, IntPtr hWndNext);

    const int WM_DRAWCLIPBOARD = 0x0308;
    const int WM_CHANGECBCHAIN = 0x030D;

    IntPtr nextHandle;
    IntPtr handle;

    HwndSource hwndSource = null;

    /// <summary>
    /// クリップボードに内容に変更があると発生します。
    /// </summary>
    public event EventHandler DrawClipboard;

    /// <summary>
    /// クリップボードの監視を開始する
    /// </summary>
    /// <param name="handle">監視に利用するウィンドウハンドル</param>
    public void StartListenClipboard(IntPtr handle)
    {
        this.hwndSource = HwndSource.FromHwnd(handle);
        this.hwndSource.AddHook(this.WndProc);
        this.handle = handle;
        this.nextHandle = SetClipboardViewer(this.handle);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_DRAWCLIPBOARD)
        {
            SendMessage(nextHandle, msg, wParam, lParam);
            this.RaiseDrawClipboard();
            handled = true;
        }
        else if (msg == WM_CHANGECBCHAIN)
        {
            if (wParam == nextHandle)
            {
                nextHandle = lParam;
            }
            else
            {
                SendMessage(nextHandle, msg, wParam, lParam);
            }
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void RaiseDrawClipboard()
    {
        if (DrawClipboard != null)
        {
            DrawClipboard(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// ClipBoardWatcherクラスを
    /// クリップボードビューアチェインから削除します。
    /// </summary>
    public void Dispose()
    {
        ChangeClipboardChain(this.handle, nextHandle);
        this.hwndSource.Dispose();
    }
}
