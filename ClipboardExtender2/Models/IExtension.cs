using System;

namespace ClipboardExtender
{
    public interface IExtension
    {
        void Initialize(string path);
        void Run(ExtensionArgs args);
    }
}
