﻿using Microsoft.Boogie;
using System.IO;

namespace DafnyLanguageServer.DafnyAccess
{
    /// <summary>
    /// OutputPrinter that does nothing aside the default functionality.
    /// Boogie requires an output printer to be set.
    /// Instead of using Dafny's Console Printer, which messes with the console,
    /// this sink is used within the Language Server.
    /// </summary>
    public class LanguageServerOutputWriterSink : OutputPrinter
    {
        public void ErrorWriteLine(TextWriter tw, string s)
        {
            tw.WriteLine(s);
        }

        public void ErrorWriteLine(TextWriter tw, string format, params object[] args)
        {
            string s = string.Format(format, args);
            this.ErrorWriteLine(tw, s);
        }

        public void AdvisoryWriteLine(string format, params object[] args)
        {
            //Since it is a sink, nothing needs to be done.
            //The method needs to be provided though, because its abstract variant is inherited.
        }

        public void Inform(string s, TextWriter tw)
        {
            if (!CommandLineOptions.Clo.Trace && !CommandLineOptions.Clo.TraceProofObligations)
                return;
            tw.WriteLine(s);
        }

        public void WriteTrailer(PipelineStatistics stats)
        {
            //Since it is a sink, nothing needs to be done.
            //The method needs to be provided though, because its abstract variant is inherited.
        }

        public void WriteErrorInformation(
              ErrorInformation errorInfo,
              TextWriter tw,
              bool skipExecutionTrace = true)
        {
            this.ReportBplError(errorInfo.Tok, errorInfo.FullMsg, true, tw);
            foreach (ErrorInformation.AuxErrorInfo auxErrorInfo in errorInfo.Aux)
            {
                if (!skipExecutionTrace || auxErrorInfo.Category == null || !auxErrorInfo.Category.Contains("Execution trace"))
                    this.ReportBplError(auxErrorInfo.Tok, auxErrorInfo.FullMsg, false, tw);
            }
            tw.Write(errorInfo.Out.ToString());
            tw.Write(errorInfo.Model.ToString());
            tw.Flush();
        }

        public virtual void ReportBplError(
          IToken tok,
          string message,
          bool error,
          TextWriter tw,
          string category = null)
        {
            if (category != null)
                message = $"{category}: {message}";
            string s;
            if (tok != null)
                s = $"{tok.filename}({tok.line},{tok.col}): {message}";
            else
                s = message;
            if (error)
                this.ErrorWriteLine(tw, s);
            else
                tw.WriteLine(s);
        }
    }
}