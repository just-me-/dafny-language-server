using System.Collections.Generic;

namespace CodeLensIntegrationTest
{
    public class ExpectedCodeLensEntry
    {
        public string Name { get; set; }
        public List<ExpectedReference> References { get; set; }

        public ExpectedCodeLensEntry()
        {
            References = new List<ExpectedReference>();
        }
    }
}