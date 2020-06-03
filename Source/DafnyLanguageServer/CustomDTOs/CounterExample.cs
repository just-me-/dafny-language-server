using System.Collections.Generic;

namespace DafnyLanguageServer.CustomDTOs
{
    public class CounterExample
    {
        public int Line { get; set; }
        public int Col { get; set; }
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();
    }
}
