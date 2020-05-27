using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Tools;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Boogie;
using Microsoft.Dafny;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Core
{
    /// <summary>
    /// This service is used by the related handler and contains the core logic.
    /// </summary>
    public class DiagnosticsProvider : IDiagnosticsProvider
    {
        private readonly ILanguageServer _router;
        private readonly MessageSenderService _msgSenderService;

        public DiagnosticsProvider(ILanguageServer router)
        {
            _router = router;
            _msgSenderService = new MessageSenderService(router);
        }

        public void SendDiagnostics(FileRepository fileRepository)
        {
            _msgSenderService.SendCurrentDocumentInProcess(fileRepository.PhysicalFile.Filepath);

            var rawDiagnosticElements = fileRepository.Result.DiagnosticElements;
            var diagnostics = ConvertToLSPDiagnostics(rawDiagnosticElements, fileRepository.PhysicalFile);
            PublishDiagnosticsParams p = new PublishDiagnosticsParams
            {
                Uri = fileRepository.PhysicalFile.Uri,
                Diagnostics = new Container<Diagnostic>(diagnostics)
            };
            _router.Document.PublishDiagnostics(p);
            _msgSenderService.SendCountedErrors(diagnostics.Count);
            fileRepository.PhysicalFile.IsValid = !(diagnostics.Count > 0);
        }

        public Collection<Diagnostic> ConvertToLSPDiagnostics(IEnumerable<DiagnosticElement> errors, PhysicalFile file)
        {
            Collection<Diagnostic> diagnostics = new Collection<Diagnostic>();
            foreach (DiagnosticElement e in errors)
            {
                var mainDiagnostic = ConvertErrorToDiagnostic(file, e);
                diagnostics.Add(mainDiagnostic);

                var relateds = ExtractRelatedInformationOfAnError(file, e);
                foreach (var r in relateds)
                {
                    diagnostics.Add(r);
                }
            }
            return diagnostics;
        }

        private Diagnostic ConvertErrorToDiagnostic(PhysicalFile file, DiagnosticElement e)
        {
            int line = e.Tok.line - 1;
            int col = e.Tok.col - 1;
            int length = file.GetLengthOfLine(line) - col;

            if (e.Msg.EndsWith("."))
            {
                e.Msg = e.Msg.Substring(0, e.Msg.Length - 1);
            }

            string msg;
            if (e.Tok.val == "anything so that it is nonnull" || e.Tok.val == null)
            {
                msg = e.Msg;
            }
            else
            {
                msg = e.Msg + $" at [ {e.Tok.val} ]";
            }

            string src = file.Filepath.Split('/').Last();

            DiagnosticSeverity severity;
            switch (e.Severity)
            {
                case ErrorLevel.Error:
                    severity = DiagnosticSeverity.Error;
                    break;
                case ErrorLevel.Warning:
                    severity = DiagnosticSeverity.Warning;
                    break;
                case ErrorLevel.Info:
                    severity = DiagnosticSeverity.Information;
                    break;
                default:
                    severity = DiagnosticSeverity.Error;
                    break;
            }

            Diagnostic d = new Diagnostic
            {
                Message = msg,
                Range = this.CreateRange(line, col, length),
                Severity = severity,
                Source = src
            };
            return d;
        }

        private IEnumerable<Diagnostic> ExtractRelatedInformationOfAnError(PhysicalFile file, DiagnosticElement e)
        {
            List<Diagnostic> relatedInformations = new List<Diagnostic>();
            foreach (ErrorInformation.AuxErrorInfo aux in e.Aux)
            {
                if (aux.Category == "Execution trace")
                {
                    continue;
                }
                string auxmessage = aux.Msg;
                int auxline = aux.Tok.line - 1;
                int auxcol = aux.Tok.col - 1;
                int auxlength = file.GetLengthOfLine(auxline) - auxcol;
                Range auxrange = CreateRange(auxline, auxcol, auxlength);

                string src = file.FileName;
                Diagnostic relatedDiagnostic = new Diagnostic
                {
                    Message = auxmessage,
                    Range = auxrange,
                    Severity = DiagnosticSeverity.Information,
                    Source = src
                };

                relatedInformations.Add(relatedDiagnostic);
            }
            return relatedInformations;
        }

        private Range CreateRange(long line, long chr, long length)
        {
            if (length < 0)
            {
                length = Math.Abs(length);
                chr -= length;
            }
            Position start = new Position(line, chr);
            Position end = new Position(line, chr + length);
            return new Range(start, end);
        }
    }
}
