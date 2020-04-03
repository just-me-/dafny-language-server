using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.DafnyAccess;
using Microsoft.Boogie;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Services
{
    /// <summary>
    /// This service is used by the <c>TextDocumentSyncHandler</c> to provide verification for Dafny files with <c>SendDiagnostics</c>. 
    /// </summary>
    public class DiagnosticsService
    {
        private readonly ILanguageServer _router;
        private readonly MessageSenderService _msgSenderService; 

        public DiagnosticsService(ILanguageServer router)
        {
            _router = router;
            _msgSenderService = new MessageSenderService(router);
        }

        public void SendDiagnostics(FileRepository fileRepository)
        {
            _msgSenderService.SendCurrentDocumentInProcess(fileRepository.PhysicalFile.Filepath);
            try
            {
                var errors = fileRepository.Result.Errors;
                var diagnostics = CreateDafnyDiagnostics(errors, fileRepository.PhysicalFile);
                PublishDiagnosticsParams p = new PublishDiagnosticsParams
                {
                    Uri = fileRepository.PhysicalFile.Uri,
                    Diagnostics = new Container<Diagnostic>(diagnostics)
                };
                _router.Document.PublishDiagnostics(p);
                _msgSenderService.SendCountedErrors(diagnostics.Count);
            } catch (Exception e)
            {
               _msgSenderService.SendError("Error while Verifying." + e.Message);
            }
        }

        public Collection<Diagnostic> CreateDafnyDiagnostics(IEnumerable<DiagnosticError> errors, PhysicalFile file)
        {
            Collection<Diagnostic> diagnostics = new Collection<Diagnostic>();
            foreach (DiagnosticError e in errors)
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

        private Diagnostic ConvertErrorToDiagnostic(PhysicalFile file, DiagnosticError e)
        {
            int line = e.Tok.line - 1;
            int col = e.Tok.col - 1;
            int length = FileHelper.GetLineLength(file.Sourcecode, line) - col;

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
            Diagnostic d = new Diagnostic
            {
                Message = msg,
                Range = FileHelper.CreateRange(line, col, length),
                Severity = DiagnosticSeverity.Error,
                Source = src
            };
            return d;
        }

        private List<Diagnostic> ExtractRelatedInformationOfAnError(PhysicalFile file, DiagnosticError e)
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
                int auxlength = FileHelper.GetLineLength(file.Sourcecode, auxline) - auxcol;
                Range auxrange = FileHelper.CreateRange(auxline, auxcol, auxlength);

                string src = file.Filepath.Split('/').Last();
                Diagnostic relatedDiagnostic = new Diagnostic()
                {
                    Message = auxmessage,
                    Range = auxrange,
                    Severity = DiagnosticSeverity.Information,
                    Source = file.Filepath
                };

                relatedInformations.Add(relatedDiagnostic);
            }
            return relatedInformations;
        }
    }
}
