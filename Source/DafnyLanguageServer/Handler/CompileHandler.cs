using DafnyLanguageServer.Services;
using OmniSharp.Extensions.JsonRpc;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace DafnyLanguageServer.Handler
{
    public class CompilerParams : IRequest<CompilerResults>
    {
        public string FileToCompile { get; set; }
        public string[] CompilationArguments { get; set; }
    }

    public class CompilerResults
    {
        public bool Error { get; set; }
        public string Message { get; set; }
        public bool? Executable { get; set; }
    }

    [Serial, Method("compile")]
    public interface ICompile : IJsonRpcRequestHandler<CompilerParams, CompilerResults> { }

    public class CompileHandler : ICompile
    {
        public async Task<CompilerResults> Handle(CompilerParams request, CancellationToken cancellationToken)
        {
            CompilationService cs = new CompilationService(request.FileToCompile, request.CompilationArguments);
            return await cs.Compile();
        }
    }
}
