using System;
using DafnyLanguageServer.DafnyAccess;
using OmniSharp.Extensions.JsonRpc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DafnyLanguageServer.Tools;
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

        public CounterExampleHandler(ILanguageServer router, Workspace b, ILoggerFactory loggerFactory)
        : base(router, b, loggerFactory)
        {
        }

        public async Task<CounterExampleResults> Handle(CounterExampleParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation("Handling Counter Example"); // todo lang file #102

            try
            {
                var file = _workspaceManager.GetFileRepository(request.DafnyFile);
                return await Task.Run(() => file.CounterExample());
            }
            catch (Exception e)
            {
                HandleError("Error while processing counter examper request", e); //todo lang
                return new CounterExampleResults();
            }
        }

    }
}