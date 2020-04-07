using DafnyLanguageServer.DafnyAccess;
using System;
using System.Collections.Generic;
using MSBoodie = Microsoft.Boogie;

namespace DafnyLanguageServer.FileManager
{
    public class TranslationResult
    {
        public List<DiagnosticError> Errors { get; set;  } = new List<DiagnosticError>(); // behaltem

        public Microsoft.Dafny.Program DafnyProgram { get; set; } // behalten 
        public IEnumerable<Tuple<string, MSBoodie.Program>> BoogiePrograms { get; set; }  // behalten 

        // Keep track of the process state todo #148

        public TranslationStatus TranslationStatus { get; set;  }
    }
}
