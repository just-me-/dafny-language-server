using DafnyLanguageServer.Commons;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DafnyLanguageServer.Core
{
    public interface IDiagnosticsProvider
    {
        PublishDiagnosticsParams GetPublishableDiagnostics();
        Collection<Diagnostic> ConvertToLSPDiagnostics(IEnumerable<DiagnosticElement> errors);
    }
}