using System.IO;

namespace CompileHandlerTest
{
    internal static class PathConstants
    {

        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(PathConstants).Assembly.Location);
        internal static readonly string testPath = Path.GetFullPath(Path.Combine(assemblyPath, "../Test/compileHandlerFiles"));
        internal static readonly string dafnyExe = Path.GetFullPath(Path.Combine(assemblyPath, "../Binaries/Dafny.exe"));

        internal static readonly string fineDLLOutput = "fineDLL.dll";
        internal static readonly string fineEXEOutput = "fineEXE.exe";


        internal static readonly string dfy_fineDLL = "fineDLL.dfy";
        internal static readonly string dfy_fineEXE = "fineEXE.dfy";
        internal static readonly string dfy_assertion = "assertion.dfy";
        internal static readonly string dfy_identifier = "identifier.dfy";
        internal static readonly string dfy_postcondition = "postcondition.dfy";
    }
}
