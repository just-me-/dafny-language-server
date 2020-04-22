using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;

namespace DafnyLanguageServer.DafnyAccess
{
    // Decompiled with JetBrains decompiler
    // Type: Microsoft.Boogie.ConsolePrinter
    // Assembly: BoogieExecutionEngine, Version=2.4.1.10503, Culture=neutral, PublicKeyToken=736440c9b414ea16
    // MVID: ACE21C81-810D-4A55-A367-D03CF4F40F3F
    // Assembly location: G:\Dokumente\VisualStudio\BA\boogie\Binaries\BoogieExecutionEngine.dll

    using System;
    using System.IO;

 
        public class LanguageServerOutputWriter : OutputPrinter
        {
            private string File { get; }

            public LanguageServerOutputWriter(string file)
            {
                File = file;
            }
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
                using (StreamWriter sw = new StreamWriter(File, true))
                {
                    sw.WriteLine(format, args);
                }
            }

            public void Inform(string s, TextWriter tw)
            {
                if (!CommandLineOptions.Clo.Trace && !CommandLineOptions.Clo.TraceProofObligations)
                    return;
                tw.WriteLine(s);
            }

            public void WriteTrailer(PipelineStatistics stats)
            {
                using (StreamWriter sw = new StreamWriter(File, true))
                {
                    sw.WriteLine();
                    if (CommandLineOptions.Clo.vcVariety == CommandLineOptions.VCVariety.Doomed)
                        sw.Write("{0} finished with {1} credible, {2} doomed{3}", new object[4]
                        {
                            (object) CommandLineOptions.Clo.DescriptiveToolName,
                            (object) stats.VerifiedCount,
                            (object) stats.ErrorCount,
                            stats.ErrorCount == 1 ? (object) "" : (object) "s"
                        });
                    else if (CommandLineOptions.Clo.ShowVerifiedProcedureCount)
                        sw.Write("{0} finished with {1} verified, {2} error{3}", new object[4]
                        {
                            (object) CommandLineOptions.Clo.DescriptiveToolName,
                            (object) stats.VerifiedCount,
                            (object) stats.ErrorCount,
                            stats.ErrorCount == 1 ? (object) "" : (object) "s"
                        });
                    else
                        sw.Write("{0} finished with {1} error{2}",
                            (object) CommandLineOptions.Clo.DescriptiveToolName, (object) stats.ErrorCount,
                            stats.ErrorCount == 1 ? (object) "" : (object) "s");
                    if ((uint) stats.InconclusiveCount > 0U)
                        sw.Write(", {0} inconclusive{1}", (object) stats.InconclusiveCount,
                            stats.InconclusiveCount == 1 ? (object) "" : (object) "s");
                    if ((uint) stats.TimeoutCount > 0U)
                        sw.Write(", {0} time out{1}", (object) stats.TimeoutCount,
                            stats.TimeoutCount == 1 ? (object) "" : (object) "s");
                    if ((uint) stats.OutOfMemoryCount > 0U)
                        sw.Write(", {0} out of memory", (object) stats.OutOfMemoryCount);
                    if ((uint) stats.OutOfResourceCount > 0U)
                        sw.Write(", {0} out of resource", (object) stats.OutOfResourceCount);
                    sw.WriteLine();
                    sw.Flush();
                }
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
