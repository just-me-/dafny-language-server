using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Resources
{
    public class FileAndFolderLocator
    {

        //Base Path
        private static readonly string thisAssemblyPath = Path.GetDirectoryName(typeof(FileAndFolderLocator).Assembly.Location);


        //Folders
        public static readonly string rootFolder = Combine(thisAssemblyPath, "../");
        public static readonly string configPath = CombineWithRoot("Config");
        public static readonly string testFilesPath = CombineWithRoot("Test/LanguageServerTest/DafnyFiles");

        //Executables
        public static readonly string dafnyExe = CombineWithRoot("Binaries/Dafny.exe");
        public static readonly string langServExe = CombineWithRoot("Binaries/DafnyLanguageServer.exe");


        
        
        private static string Combine(string a, string b) => Path.GetFullPath(Path.Combine(a, b));
        private static string CombineWithRoot(string b) => Combine(rootFolder, b);
    }
}
