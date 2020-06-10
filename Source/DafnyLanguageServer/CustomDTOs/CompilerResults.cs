namespace DafnyLanguageServer.CustomDTOs
{
    public class CompilerResults
    {
        public bool Error { get; set; }
        public string Message { get; set; }
        public bool? Executable { get; set; }
    }
}