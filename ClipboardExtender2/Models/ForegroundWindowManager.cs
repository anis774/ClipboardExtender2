using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reactive.Linq;

namespace ClipboardExtender2.Models
{
    public class ForegroundWindowManager : IDisposable
    {
        private IDisposable watchObserver;
        private IEnumerable<IntPtr> ignoreHandles;

        public ForegroundWindowManager(IEnumerable<IntPtr> ignoreHandles)
        {
            this.ignoreHandles = ignoreHandles;

            this.watchObserver = Observable.Interval(TimeSpan.FromMilliseconds(200))
                .Subscribe(
                    _ =>
                    {
                        var foregroundWindowHandle = GetForegroundWindow();
                        if (!this.ignoreHandles.Contains(foregroundWindowHandle))
                        {
                            this._ForegroundWindowHandle = foregroundWindowHandle;
                        }
                    });
        }

        private IntPtr _ForegroundWindowHandle;
        public IntPtr ForegroundWindowHandle
        {
            get
            {
                return this._ForegroundWindowHandle;
            }
        }

        public bool RollbackForegroundWindow()
        {
            return SetForegroundWindow(this.ForegroundWindowHandle);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public void Dispose()
        {
            this.watchObserver.Dispose();
        }
    }
}
