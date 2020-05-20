﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using DafnyLanguageServer.Commons;
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
