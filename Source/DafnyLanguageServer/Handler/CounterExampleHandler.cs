using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.Services;
using OmniSharp.Extensions.JsonRpc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace DafnyLanguageServer.Handler
{
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

        private readonly BufferManager _bufferManager;

        public CounterExampleHandler(BufferManager b)
        {
            _bufferManager = b;
        }

        public async Task<CounterExampleResults> Handle(CounterExampleParams request, CancellationToken cancellationToken)
        {
            var file = _bufferManager.GetFile(request.DafnyFile);
            // Counterexample needs a DafnyTranslationUnit with this current request DafnyFile path
            // therefore do not use file.DafnyTranslationUnit
            var dafnyTranslationUnit = new DafnyTranslationUnit(request.DafnyFile, file.Sourcecode);

            return await Task.Run(()=>dafnyTranslationUnit.CounterExample());
        }

    }
}