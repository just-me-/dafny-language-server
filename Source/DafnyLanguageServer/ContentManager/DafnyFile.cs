using DafnyLanguageServer.DafnyAccess;
using System;
using System.IO;

namespace DafnyLanguageServer.ContentManager
{
    /// <summary>
    /// This class represents a Dafny file. A Dafny file contains attributes like Uri, Dafny source code and a (buffered) symbol table for that source code.
    /// A <c>DafnyFile</c> has also its own <c>DafnyTranslationUnit</c> to verify this Dafny file and provide related information.
    /// </summary>
    public class DafnyFile
    {
        public Uri Uri { get; set; }
        public string Filepath => Uri.ToString();
        public string Sourcecode { get; set; }
        public FileSymboltable Symboltable { get; set; }
        public IDafnyTranslationUnit DafnyTranslationUnit { get; set; }
    }
}
