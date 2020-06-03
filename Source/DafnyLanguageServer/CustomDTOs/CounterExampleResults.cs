using System.Collections.Generic;

namespace DafnyLanguageServer.CustomDTOs
{
    public class CounterExampleResults
    {
        public List<CounterExample> CounterExamples { get; } = new List<CounterExample>();
    }
}
