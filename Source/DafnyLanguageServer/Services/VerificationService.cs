using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
                var d = ConvertErrorToDiagnostic(filepath, sourcecode, e);
                d.RelatedInformation = GetRelatedInformationForAnError(sourcecode, e);
                diagnostics.Add(d);
            }

            return diagnostics;
        }

        private static Container<DiagnosticRelatedInformation> GetRelatedInformationForAnError(string sourcecode, DiagnosticError e)
        {
            List<DiagnosticRelatedInformation> relatedInformations = new List<DiagnosticRelatedInformation>();
            for (int i = 0; i < e.Aux.Count - 1; i++) //ignore last element (trace)
            {
                int auxline = e.Aux[i].Tok.line - 1;
                int auxcol = e.Aux[i].Tok.col - 1;
                int auxlength = FileHelper.GetLineLength(sourcecode, auxline) - auxcol;
                Range auxrange = FileHelper.CreateRange(auxline, auxcol, auxlength);
                Location auxlocation = new Location()
                {
                    Range = auxrange,
                    Uri = File.Exists(e.Aux[i].Tok.filename) ? new Uri(e.Aux[i].Tok.filename) : null
                };

                string auxmessage = e.Aux[i].Msg;

                DiagnosticRelatedInformation relatedDiagnostic = new DiagnosticRelatedInformation()
                {
                    Location = auxlocation,
                    Message = auxmessage
                };

                relatedInformations.Add(relatedDiagnostic);
            }

            return new Container<DiagnosticRelatedInformation>(relatedInformations);
        }

        private static Diagnostic ConvertErrorToDiagnostic(string filepath, string sourcecode, DiagnosticError e)
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
            return d;
        }
    }
}
