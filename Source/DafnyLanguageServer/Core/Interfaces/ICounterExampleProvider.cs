using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Handler;

namespace DafnyLanguageServer.Core
{
    public interface ICounterExampleProvider
    {
        CounterExampleResults LoadCounterModel();
    }
}
