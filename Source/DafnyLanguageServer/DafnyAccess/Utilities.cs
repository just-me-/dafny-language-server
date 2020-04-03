using Microsoft.Boogie;
using Microsoft.Dafny;
using System;

namespace DafnyLanguageServer.DafnyAccess
{


    /// <summary>
    /// This class is used to apply arguments to the Dafny Engine.
    /// Is Copy Pasted from DafnyServer.Utilities because they are not public.
    /// </summary>
    class ServerUtils
    {

        internal static void ApplyArgs(string[] args, ErrorReporter reporter)
        {
            Microsoft.Dafny.DafnyOptions.Install(new Microsoft.Dafny.DafnyOptions(reporter));
            Microsoft.Dafny.DafnyOptions.O.ProverKillTime = 10; //This is just a default; it can be overriden
            DafnyOptions.O.VerifySnapshots = 3;

            if (CommandLineOptions.Clo.Parse(args))
            {
                DafnyOptions.O.VcsCores = Math.Max(1, System.Environment.ProcessorCount / 2); // Don't use too many cores
                DafnyOptions.O.PrintTooltips = true; // Dump tooptips (ErrorLevel.Info) to stdout
                                                     //DafnyOptions.O.UnicodeOutput = true; // Use pretty warning signs
                DafnyOptions.O.TraceProofObligations = true; // Show which method is being verified, but don't show duration of verification
            }
            else
            {
                throw new Exception("Invalid command line options");
            }
        }
    }
}
