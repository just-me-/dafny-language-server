using Microsoft.Boogie;
using System.Collections.Generic;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Handler;

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
