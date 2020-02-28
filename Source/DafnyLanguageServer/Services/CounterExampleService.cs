using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.Handler;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Services
{
    public class CounterExampleService
    {
        private IDafnyTranslationUnit _dafnyTranslationUnit;

        public CounterExampleService(IDafnyTranslationUnit dafnyTranslationUnit)
        {
            _dafnyTranslationUnit = dafnyTranslationUnit;
        }

        public Task<CounterExampleResults> ProvideCounterExamples()
        {
            return Task.Run(() =>
            {
                var allCounterExamplesReturnContainer = new CounterExampleResults();
                var models = _dafnyTranslationUnit.CounterExample();

                if (models.Count == 0)
                {
                    return allCounterExamplesReturnContainer;
                }

                var states = models[0].States;

                for (int i = 2; i < states.Count; i++)
                {
                    var entry = states[i];
                    var variables = entry.Variables;

                    CounterExampleResult currentCounterExample = new CounterExampleResult();

                    currentCounterExample.Col = entry.Column;
                    currentCounterExample.Line = entry.Line;

                    foreach (var variable in variables)
                    {
                        currentCounterExample.Variables.Add(variable.Name, variable.Value);
                    }

                    allCounterExamplesReturnContainer.CounterExamples.Add(currentCounterExample);
                }
                return allCounterExamplesReturnContainer;
            });
        }
    }
}
