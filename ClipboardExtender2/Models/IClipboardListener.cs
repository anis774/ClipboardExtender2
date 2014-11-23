using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardExtender2.Models
{
    public interface IClipboardListener: IDisposable
    {
        void Start();
        event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;
    }
}
