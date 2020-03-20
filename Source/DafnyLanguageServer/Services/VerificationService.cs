using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.DafnyAccess;
using Microsoft.Boogie;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Services
{
    public class VerificationService
    {
        private readonly ILanguageServer _router;
        private readonly MessageSender _msgSender; 

        public VerificationService(ILanguageServer router)
        {
            _router = router;
            _msgSender = new MessageSender(router);
        }

        public void Verify(DafnyFile file)
        {
            _msgSender.SendCurrentDocumentInProcess(file.Filepath);
            try
            {
                var errors = file.DafnyTranslationUnit.GetErrors();
                var diagnostics = CreateDafnyDiagnostics(errors, file.Filepath, file.Sourcecode);

                PublishDiagnosticsParams p = new PublishDiagnosticsParams
                {
                    Uri = file.Uri,
                    Diagnostics = new Container<Diagnostic>(diagnostics)
                };
                _router.Document.PublishDiagnostics(p);
                _msgSender.SendErrornumber(diagnostics.Count);
            } catch (Exception e)
            {
                Console.WriteLine("There was an error: " + e);
            }
        }

        public Collection<Diagnostic> CreateDafnyDiagnostics(IEnumerable<DiagnosticError> errors, string filepath, string sourcecode = null)
        {
            if (sourcecode is null)
            {
                sourcecode = File.ReadAllText(filepath);
            }

            Collection<Diagnostic> diagnostics = new Collection<Diagnostic>();

            foreach (DiagnosticError e in errors)
            {
                var mainDiagnostic = ConvertErrorToDiagnostic(filepath, sourcecode, e);
                diagnostics.Add(mainDiagnostic);

                var relateds = ExtractRelatedInformationOfAnError(filepath, sourcecode, e);
                foreach (var r in relateds)
                {
                    diagnostics.Add(r);
                }
            }

            return diagnostics;
        }


        private Diagnostic ConvertErrorToDiagnostic(string filepath, string sourcecode, DiagnosticError e)
        {
            int line = e.Tok.line - 1;
            int col = e.Tok.col - 1;
            int length = FileHelper.GetLineLength(sourcecode, line) - col;

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
            Diagnostic d = new Diagnostic
            {
                Message = msg,
                Range = FileHelper.CreateRange(line, col, length),
                Severity = DiagnosticSeverity.Error,
                Source = filepath
            };
            return d;
        }

        private List<Diagnostic> ExtractRelatedInformationOfAnError(string filepath, string sourcecode, DiagnosticError e)
        {
            List<Diagnostic> relatedInformations = new List<Diagnostic>();
            foreach (ErrorInformation.AuxErrorInfo aux in e.Aux)
            {
                if (aux.Category == "Related location")
                {
                    continue;
                }
                string auxmessage = aux.Msg;
                int auxline = aux.Tok.line - 1;
                int auxcol = aux.Tok.col - 1;
                int auxlength = FileHelper.GetLineLength(sourcecode, auxline) - auxcol;
                Range auxrange = FileHelper.CreateRange(auxline, auxcol, auxlength);

                Diagnostic relatedDiagnostic = new Diagnostic()
                {
                    Message = auxmessage,
                    Range = auxrange,
                    Severity = DiagnosticSeverity.Information,
                    Source = filepath
                };

                relatedInformations.Add(relatedDiagnostic);
            }

            return relatedInformations;
        }
    }
}
