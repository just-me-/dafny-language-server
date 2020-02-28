using DafnyLanguageServer.Services;
using OmniSharp.Extensions.Embedded.MediatR;
using OmniSharp.Extensions.JsonRpc;
using System.Threading;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Handler
{
    public class CompilerParams : IRequest<CompilerResults>
    {
        public string DafnyFilePath { get; set; }
        public string DafnyExePath { get; set; }
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
            CompilationService cs = new CompilationService(request.DafnyExePath, request.DafnyFilePath);
            return await cs.Compile();
        }
    }
}
