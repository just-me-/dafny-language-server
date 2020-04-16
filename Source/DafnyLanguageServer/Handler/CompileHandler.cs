using OmniSharp.Extensions.JsonRpc;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.HandlerServices;
using MediatR;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides an extension of the Language Server Protocol.
    /// It gets triggered every time a client sends a <c>compile</c> command request.
    /// This class creates a new <c>CompilationService</c> to provide compiled results. 
    /// </summary>
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


        private readonly WorkspaceManager _workspaceManager;

        public CompileHandler(WorkspaceManager b)
        {
            _workspaceManager = b;
        }

        public async Task<CompilerResults> Handle(CompilerParams request, CancellationToken cancellationToken)
        {
            FileRepository f = _workspaceManager.GetFileRepository(request.FileToCompile);
            return await Task.Run(() => f.Compile(request.CompilationArguments), cancellationToken);
        }
    }
}
