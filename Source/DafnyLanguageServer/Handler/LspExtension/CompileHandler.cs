using System;
using OmniSharp.Extensions.JsonRpc;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.CustomDTOs;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides an extension of the Language Server Protocol.
    /// It gets triggered every time a client sends a <c>compile</c> command request.
    /// This class creates a new <c>CompilationService</c> to provide compiled results. 
    /// </summary>
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
                IFileRepository f = _workspaceManager.GetFileRepository(request.FileToCompile);
                string[] args = request.CompilationArguments;
                ICompileProvider provider = new CompileProvider(f, args);
                return await Task.Run(() => provider.Compile(), cancellationToken);
            }
            catch (Exception e)
            {
                HandleError(string.Format(Resources.LoggingMessages.request_error, _method), e);
                return null;
            }
        }
    }
}
