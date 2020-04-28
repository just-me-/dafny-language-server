using System;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.DafnyAccess;
using OmniSharp.Extensions.JsonRpc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

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

    public class CounterExampleHandler : ICounterExample
    {
        private readonly WorkspaceManager _workspaceManager;
        private readonly ILogger _log;

        public CounterExampleHandler(WorkspaceManager b, ILoggerFactory loggerFactory)
        {
            _workspaceManager = b;
            _log = loggerFactory.CreateLogger("");
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
                _log.LogError("Internal server error handling Counter Example: " + e.Message); // todo lang file #102

                return null; //todo warum return null... ght dat ned eleganter? sendError oder so via new throw ? #107
            }
        }

    }
}