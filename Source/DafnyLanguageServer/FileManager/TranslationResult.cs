using DafnyLanguageServer.DafnyAccess;
using System;
using System.Collections.Generic;
using MSBoodie = Microsoft.Boogie;

namespace DafnyLanguageServer.FileManager
{
    public class TranslationResult
    {
        public List<DiagnosticError> Errors { get; set;  } = new List<DiagnosticError>();

        public Microsoft.Dafny.Program DafnyProgram { get; set; }
        public IEnumerable<Tuple<string, MSBoodie.Program>> BoogiePrograms { get; set; }

        // Keep track of the process state todo #148

        public TranslationStatus TranslationStatus { get; set;  }
    }
}
