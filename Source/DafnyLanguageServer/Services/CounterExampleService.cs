using System.Globalization;
using System.Text.RegularExpressions;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.Handler;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Services
{
    public class CounterExampleService
    {
        private readonly IDafnyTranslationUnit _dafnyTranslationUnit;

        public CounterExampleService(IDafnyTranslationUnit dafnyTranslationUnit)
        {
            _dafnyTranslationUnit = dafnyTranslationUnit;
        }

        public Task<CounterExampleResults> ProvideCounterExamples()
        {
            return Task.Run(() =>
            {
                return _dafnyTranslationUnit.CounterExample();
            });
        }
    }
}
