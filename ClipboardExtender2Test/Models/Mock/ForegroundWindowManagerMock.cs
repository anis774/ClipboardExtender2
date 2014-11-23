using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClipboardExtender2.Models;

namespace ClipboardExtender2Test.Models.Mock
{
    public class ForegroundWindowManagerMock: IForegroundWindowManager
    {
        public bool Disposed { get; set; }
        public void Dispose()
        {
            this.Disposed = true;
        }

        public IntPtr ForegroundWindowHandle { get; set; }

        public bool CalledRollbackForegroundWindowCalled;
        public bool RollbackForegroundWindow()
        {
            this.CalledRollbackForegroundWindowCalled = true;
            return true;
        }
    }
}
