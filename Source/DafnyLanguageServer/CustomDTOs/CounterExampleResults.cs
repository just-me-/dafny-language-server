using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.CustomDTOs
{
    public class CounterExampleResults
    {
        public List<CounterExample> CounterExamples { get; } = new List<CounterExample>();
    }
}
