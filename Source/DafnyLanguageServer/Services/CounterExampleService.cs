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
                        //int parsing
                        string regex = @"\(+(-?) ?(\d+)\)+"; //to parse like ((- 24)) or ((0))
                        MatchCollection matchCollection = Regex.Matches(variable.Value, regex);
                        if (matchCollection.Count == 1)
                        {
                            var match = matchCollection[0];
                            bool isNegative = match.Groups[1].Value == "-";
                            int value = int.Parse(match.Groups[2].Value);
                            value = isNegative ? -value : value;
                            currentCounterExample.Variables.Add(variable.Name, value.ToString());
                            continue;
                        }

                        //float parsing
                        regex = @"\(+(-?) ?(\d+\.\d+)\)+"; //to parse like ((- 24.0)) or ((0.0))
                        matchCollection = Regex.Matches(variable.Value, regex);
                        if (matchCollection.Count == 1)
                        {
                            var match = matchCollection[0];
                            bool isNegative = match.Groups[1].Value == "-";
                            float value = float.Parse(match.Groups[2].Value);
                            value = isNegative ? -value : value;
                            currentCounterExample.Variables.Add(variable.Name,
                                value.ToString(CultureInfo.CurrentCulture));
                            continue;
                        }

                        currentCounterExample.Variables.Add(variable.Name, variable.Value);
                        
                        
                    }

                    allCounterExamplesReturnContainer.CounterExamples.Add(currentCounterExample);
                }
                return allCounterExamplesReturnContainer;
            });
        }
    }
}
