using System;
namespace ClipboardExtender2.Models
{
    public interface IForegroundWindowManager
    {
        void Dispose();
        IntPtr ForegroundWindowHandle { get; }
        bool RollbackForegroundWindow();
    }
}
