using DafnyLanguageServer.DafnyAccess;
using System;
using System.IO;

namespace DafnyLanguageServer.ContentManager
{
    public class DafnyFile
    {
        public Uri Uri { get; set; }
        public string Filepath => Uri.LocalPath;
        public string Sourcecode { get; set; }
        public FileSymboltable Symboltable { get; set; }
        public IDafnyTranslationUnit DafnyTranslationUnit { get; set; }


    }
}
