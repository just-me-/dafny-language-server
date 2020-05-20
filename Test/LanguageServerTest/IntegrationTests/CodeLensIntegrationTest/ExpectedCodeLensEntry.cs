using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
