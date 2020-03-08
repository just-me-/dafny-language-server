using System.IO;

namespace PathConstants
{
    public static class PathConstants
    {

        public static readonly string assemblyPath = Path.GetDirectoryName(typeof(PathConstants).Assembly.Location);
        public static readonly string rootPath = Path.GetFullPath(Path.Combine(assemblyPath, "../"));
        public static readonly string testFilesPath = Path.GetFullPath(Path.Combine(rootPath, "Test/LanguageServerTest/UnitTests/CompileHandlerTest/CompileHandlerTestFiles"));
        public static readonly string dafnyExe = Path.GetFullPath(Path.Combine(rootPath, "Binaries/Dafny.exe"));

        public static readonly string fineDLLOutput = "fineDLL.dll";
        public static readonly string fineEXEOutput = "fineEXE.exe";


        public static readonly string dfy_fineDLL = "fineDLL.dfy";
        public static readonly string dfy_fineEXE = "fineEXE.dfy";
        public static readonly string dfy_assertion = "assertion.dfy";
        public static readonly string dfy_identifier = "identifier.dfy";
        public static readonly string dfy_postcondition = "postcondition.dfy";
    }
}