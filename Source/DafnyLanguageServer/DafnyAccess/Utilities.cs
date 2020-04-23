using Microsoft.Boogie;
using Microsoft.Dafny;
using System;

namespace DafnyLanguageServer.DafnyAccess
{
    /// <summary>
    /// This class is used to apply arguments to the Dafny Engine.
    /// Is copy-pasted from DafnyServer.Utilities because they are not public.
    /// </summary>
    class ServerUtils
    {
        internal static void ApplyArgs(string[] args, ErrorReporter reporter)
        {
            Microsoft.Dafny.DafnyOptions.Install(new Microsoft.Dafny.DafnyOptions(reporter));

            if (CommandLineOptions.Clo.Parse(args))
            {

            }
            else
            {
                throw new Exception("Invalid command line options");
            }
        }
    }
}
