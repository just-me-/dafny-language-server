using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.DafnyAccess
{
    public static class CounterExampleDefaultModelFile
    {
        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(CounterExampleModelFileTranslator).Assembly.Location);
        public static string FilePath => Path.GetFullPath(Path.Combine(assemblyPath, "../model.bvd"));

        public static void ClearDefaultModelFile()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}
