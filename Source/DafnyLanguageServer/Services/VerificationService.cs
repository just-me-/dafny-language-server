using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private IDafnyTranslationUnit _dafnyTranslationUnit;

        public VerificationService(ILanguageServer router, IDafnyTranslationUnit translationUnit)
        {
            _router = router;
            _dafnyTranslationUnit = translationUnit;
        }

        public void Verify(DafnyFile file)
        {
            // inform plugin that current document is in progress - for statusbar
            _router.Window.SendNotification("activeVerifiyingDocument", file.Filepath);
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
                SendErrornumberToClient(diagnostics.Count);
            } catch (Exception e)
            {
                Console.WriteLine("There was an error: " + e);
            }
        }

        private void SendErrornumberToClient(int counted)
        {
            _router.Window.SendNotification("updateStatusbar", counted);
        }


        public Collection<Diagnostic> CreateDafnyDiagnostics(IEnumerable<ErrorInformation> errors, string filepath, string sourcecode)
        {
            Collection<Diagnostic> diagnostics = new Collection<Diagnostic>();

            foreach (ErrorInformation e in errors)
            {
                int line = e.Tok.line - 1;
                int col = e.Tok.col - 1;
                int length = FileHelper.GetLineLength(sourcecode, line) - col;

                Diagnostic d = new Diagnostic
                {
                    Message = e.Msg + " - Hint: " + e.Tok.val,
                    Range = FileHelper.CreateRange(line, col, length),
                    Severity = DiagnosticSeverity.Error,
                    Source = filepath
                };

                for (int i = 0; i < e.Aux.Count - 1; i++) //ignore last element (trace)
                {
                    int auxline = e.Aux[i].Tok.line - 1;
                    int auxcol = e.Aux[i].Tok.col - 1;
                    int auxlength = FileHelper.GetLineLength(sourcecode, auxline) - auxcol;

                    Diagnostic relatedDiagnostic = new Diagnostic
                    {
                        Message = e.Aux[i].Msg,
                        Range = FileHelper.CreateRange(auxline, auxcol, auxlength),
                        Severity = DiagnosticSeverity.Warning,
                        Source = "The error: " + d.Message + " is the source of this warning!"
                    };

                    diagnostics.Add(relatedDiagnostic);
                }
                diagnostics.Add(d);
            }

            return diagnostics;
        }
    }
}
