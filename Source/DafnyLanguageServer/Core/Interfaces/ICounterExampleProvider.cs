using DafnyLanguageServer.Handler;

namespace DafnyLanguageServer.Core
{
    public interface ICounterExampleProvider
    {
        CounterExampleResults LoadCounterModel();
    }
}
