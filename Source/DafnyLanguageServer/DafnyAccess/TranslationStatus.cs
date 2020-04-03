using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.DafnyAccess
{
    public enum  TranslationStatus
    {
        Virgin, 
        Parsed, 
        Resolved, 
        Translated, 
        Boogied
    }
}
