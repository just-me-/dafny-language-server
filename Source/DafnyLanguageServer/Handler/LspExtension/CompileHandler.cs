using System;
using OmniSharp.Extensions.JsonRpc;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.WorkspaceManager;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

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

    public class CompileHandler : LspBasicHandler<object>, ICompile
    {
        public CompileHandler(ILanguageServer router, IWorkspace b, ILoggerFactory lf)
            : base(router, b, lf)
        {
            _method = Resources.Requests.compile;
        }

        public async Task<CompilerResults> Handle(CompilerParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation(string.Format(Resources.LoggingMessages.request_handle, _method));

            try
            {
                FileRepository f = _workspaceManager.GetFileRepository(request.FileToCompile);
                return await Task.Run(() => f.Compile(request.CompilationArguments), cancellationToken);
            }
            catch (Exception e)
            {
                HandleError(string.Format(Resources.LoggingMessages.request_error, _method), e);
                return null;
            }
        }
    }
}
