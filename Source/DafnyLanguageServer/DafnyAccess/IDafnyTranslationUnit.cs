using Microsoft.Boogie;
using System.Collections.Generic;
using DafnyLanguageServer.Handler;
using DafnyLanguageServer.FileManager;

namespace DafnyLanguageServer.DafnyAccess
{
    /// <summary>
    /// This interface is used for dependency injection. Therefore <c>TranslationUnits</c> are more isolated for easy test writing.
    /// </summary>
    public interface IDafnyTranslationUnit
    {
        TranslationResult Verify();
    }
}
