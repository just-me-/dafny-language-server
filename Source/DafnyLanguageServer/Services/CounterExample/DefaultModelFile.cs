using System.IO;
using DafnyLanguageServer.DafnyAccess;

namespace DafnyLanguageServer.Services.CounterExample
{
    public static class DefaultModelFile
    {
        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(ModelFileTranslator).Assembly.Location);
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
