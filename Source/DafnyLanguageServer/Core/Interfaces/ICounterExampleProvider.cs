using DafnyLanguageServer.CustomDTOs;

namespace DafnyLanguageServer.Core
{
    public interface ICounterExampleProvider
    {
        CounterExampleResults LoadCounterModel();
    }
}
