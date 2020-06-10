using MediatR;

namespace DafnyLanguageServer.CustomDTOs
{
    public class CompilerParams : IRequest<CompilerResults>
    {
        public string FileToCompile { get; set; }
        public string[] CompilationArguments { get; set; }
    }
}