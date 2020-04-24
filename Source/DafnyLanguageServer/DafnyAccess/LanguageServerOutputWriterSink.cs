using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;

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
            }

            public void Inform(string s, TextWriter tw)
            {
                if (!CommandLineOptions.Clo.Trace && !CommandLineOptions.Clo.TraceProofObligations)
                    return;
                tw.WriteLine(s);
            }

            public void WriteTrailer(PipelineStatistics stats)
            {
                
            }

            public void WriteErrorInformation(
              ErrorInformation errorInfo,
              TextWriter tw,
              bool skipExecutionTrace = true)
            {
                this.ReportBplError(errorInfo.Tok, errorInfo.FullMsg, true, tw, (string)null);
                foreach (ErrorInformation.AuxErrorInfo auxErrorInfo in errorInfo.Aux)
                {
                    if (!skipExecutionTrace || auxErrorInfo.Category == null || !auxErrorInfo.Category.Contains("Execution trace"))
                        this.ReportBplError(auxErrorInfo.Tok, auxErrorInfo.FullMsg, false, tw, (string)null);
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
                    message = string.Format("{0}: {1}", (object)category, (object)message);
                string s;
                if (tok != null)
                    s = string.Format("{0}({1},{2}): {3}", (object)tok.filename, (object)tok.line, (object)tok.col, (object)message);
                else
                    s = message;
                if (error)
                    this.ErrorWriteLine(tw, s);
                else
                    tw.WriteLine(s);
            }
        }
    

}
