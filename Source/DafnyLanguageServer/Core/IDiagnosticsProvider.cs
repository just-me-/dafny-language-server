using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Handler;
using DafnyLanguageServer.WorkspaceManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    public interface IDiagnosticsProvider
    {
        void SendDiagnostics(FileRepository fileRepository);
        Collection<Diagnostic> ConvertToLSPDiagnostics(IEnumerable<DiagnosticElement> errors, PhysicalFile file);
    }
}
