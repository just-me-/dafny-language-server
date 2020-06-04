using System.IO;

namespace DafnyLanguageServer.Commons
{
    public static class FileAndFolderLocations
    {
        //Dafny-Testfiles are referenced in the "TestCommons/TestCommons/TestFilePaths.cs File to keep this one concise.

        //Base Path
        private static readonly string thisAssemblyPath = Path.GetDirectoryName(typeof(FileAndFolderLocations).Assembly.Location);

        //Folders
        public static readonly string rootFolder = Combine(thisAssemblyPath, "../");
        public static readonly string configFolder = CombineWithRoot("Config");
        public static readonly string logFolder = CombineWithRoot("Logs");
        public static readonly string testFilesFolder = CombineWithRoot("Test/LanguageServerTest/DafnyFiles");

        //Executables
        public static readonly string dafnyExe = CombineWithRoot("Binaries/Dafny.exe");
        public static readonly string langServExe = CombineWithRoot("Binaries/DafnyLanguageServer.exe");

        //Config Files
        public static readonly string reservedWordList = Combine(configFolder, "ReservedDafnyWords.json");
        public static readonly string languageServerJSONConfigFile = Combine(configFolder, "LanguageServerConfig.json");

        //Logs and Stream Redirection
        public static readonly string defaultStreamFile = Combine(logFolder, "LanguageServerStreamRedirection.txt");
        public static readonly string defaultLogFile = Combine(logFolder, "LanguageServerLog.txt");

        //model.bvd for Counter Example
        public static readonly string modelBVD = CombineWithRoot("model.bvd");


        private static string Combine(string a, string b) => Path.GetFullPath(Path.Combine(a, b));
        private static string CombineWithRoot(string b) => Combine(rootFolder, b);
    }
}