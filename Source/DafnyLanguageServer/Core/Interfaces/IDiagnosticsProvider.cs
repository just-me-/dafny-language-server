using System.Collections.Generic;
using System.Collections.ObjectModel;
using DafnyLanguageServer.Commons;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    public interface IDiagnosticsProvider
    {
        PublishDiagnosticsParams GetPublishableDiagnostics();
        Collection<Diagnostic> ConvertToLSPDiagnostics(IEnumerable<DiagnosticElement> errors);
    }
}
