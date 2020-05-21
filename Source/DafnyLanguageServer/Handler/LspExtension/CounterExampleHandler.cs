using System;
using OmniSharp.Extensions.JsonRpc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.WorkspaceManager;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides an extension of the Language Server Protocol.
    /// It gets triggered every time a client sends a <c>counterExample</c> command request.
    /// This class creates a new <c>DafnyTranslationUnit</c> to provide counter example result. 
    /// </summary>
    public class CounterExampleParams : IRequest<CounterExampleResults>
    {
        public string DafnyFile { get; set; }
    }

    public class CounterExample
    {
        public int Line { get; set; }
        public int Col { get; set; }
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();
    }

    public class CounterExampleResults
    {
        public List<CounterExample> CounterExamples { get; } = new List<CounterExample>();
    }

    [Serial, Method("counterExample")]
    public interface ICounterExample : IJsonRpcRequestHandler<CounterExampleParams, CounterExampleResults>
    {
    }

    public class CounterExampleHandler : LspBasicHandler<object>, ICounterExample
    {

        public CounterExampleHandler(ILanguageServer router, IWorkspace b, ILoggerFactory loggerFactory)
        : base(router, b, loggerFactory)
        {
            _method = Resources.Requests.counterExample;

        }

        public async Task<CounterExampleResults> Handle(CounterExampleParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation(string.Format(Resources.LoggingMessages.request_handle, _method));


            try
            {
                var file = _workspaceManager.GetFileRepository(request.DafnyFile);
                return await Task.Run(() => file.CounterExample(), cancellationToken);
            }
            catch (Exception e)
            {
                HandleError(string.Format(Resources.LoggingMessages.request_error, _method), e);
                return new CounterExampleResults();
            }
        }

    }
}