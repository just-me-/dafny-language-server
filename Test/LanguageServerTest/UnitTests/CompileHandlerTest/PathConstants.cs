using System.IO;

namespace CompileHandlerTest
{
    internal static class PathConstants
    {

        internal static readonly string assemblyPath = Path.GetDirectoryName(typeof(PathConstants).Assembly.Location);
        internal static readonly string rootPath = Path.GetFullPath(Path.Combine(assemblyPath, "../"));
        internal static readonly string testFilesPath = Path.GetFullPath(Path.Combine(rootPath, "Test/LanguageServerTest/UnitTests/CompileHandlerTest/CompileHandlerTestFiles"));
        internal static readonly string dafnyExe = Path.GetFullPath(Path.Combine(rootPath, "Binaries/Dafny.exe"));

        internal static readonly string fineDLLOutput = "fineDLL.dll";
        internal static readonly string fineEXEOutput = "fineEXE.exe";


        internal static readonly string dfy_fineDLL = "fineDLL.dfy";
        internal static readonly string dfy_fineEXE = "fineEXE.dfy";
        internal static readonly string dfy_assertion = "assertion.dfy";
        internal static readonly string dfy_identifier = "identifier.dfy";
        internal static readonly string dfy_postcondition = "postcondition.dfy";
    }
}
