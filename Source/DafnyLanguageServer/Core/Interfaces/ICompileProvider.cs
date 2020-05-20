using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler;

namespace DafnyLanguageServer.Core
{
    public interface ICompileProvider
    {
        CompilerResults Compile();
    }
}
