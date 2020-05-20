﻿using System;
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
        public static readonly string configFolder = CombineWithRoot("Config");
        public static readonly string logFolder = CombineWithRoot("Logs");
        public static readonly string testFilesFolder = CombineWithRoot("Test/LanguageServerTest/DafnyFiles");

        //Executables
        public static readonly string dafnyExe = CombineWithRoot("Binaries/Dafny.exe");
        public static readonly string langServExe = CombineWithRoot("Binaries/DafnyLanguageServer.exe");

        //Config Files
        public static readonly string reservedWordList = Combine(configFolder, "ReservedDafnyWords.json");
        public static readonly string languageServerLaunchSettings = Combine(configFolder, "LanguageServerLaunchSettings.json");

        //Logs and Stream Redirection
        public static readonly string defaultStreamFile = Combine(logFolder, "LanguageServerStreamRedirection.txt");
        public static readonly string defaultLogFile = Combine(logFolder, "LanguageServerLog.txt");


        private static string Combine(string a, string b) => Path.GetFullPath(Path.Combine(a, b));
        private static string CombineWithRoot(string b) => Combine(rootFolder, b);
    }
}
