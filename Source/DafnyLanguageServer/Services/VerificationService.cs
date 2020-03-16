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
