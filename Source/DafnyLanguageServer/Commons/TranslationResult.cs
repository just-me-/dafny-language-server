using System;
using System.Collections.Generic;
using MSBoodie = Microsoft.Boogie;

namespace DafnyLanguageServer.Commons
{
    public class TranslationResult
    {
        public List<DiagnosticElement> DiagnosticElements { get; set; } = new List<DiagnosticElement>();
        public Microsoft.Dafny.Program DafnyProgram { get; set; }
        public IEnumerable<Tuple<string, MSBoodie.Program>> BoogiePrograms { get; set; }
        public TranslationStatus TranslationStatus { get; set; }
        public bool IsVerified => TranslationStatus == TranslationStatus.Verified;
    }
}