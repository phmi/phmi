using System;
using JetBrains.Annotations;

namespace PHmiConfigurator.Utils
{
    public interface ICodeWriter : IDisposable
    {
        [StringFormatMethod("line")]
        void Write(string line = null, params object[] args);

        [StringFormatMethod("line")]
        IDisposable Block(string line = null, params object[] args);
    }
}
