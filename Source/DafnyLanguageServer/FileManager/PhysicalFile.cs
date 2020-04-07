using DafnyLanguageServer.DafnyAccess;
using System;
using System.IO;

namespace DafnyLanguageServer.FileManager
{
    /// <summary>
    /// This class represents a physical Dafny file. The content of this file is not buffered.
    /// It is de current representation of the file in the users workspace in VSCode.
    /// Gets updated whenever a change event over LSP is triggered.
    /// Only contains basic attributes like source code and name. 
    /// </summary>
    public class PhysicalFile
    {
        public Uri Uri { get; set; }
        // note that his can not be just a toString of the Uri 
        public string Filepath { get; set; }
        public string Sourcecode { get; set; }
    }
}
