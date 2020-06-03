using MediatR;

namespace DafnyLanguageServer.CustomDTOs
{
    public class CounterExampleParams : IRequest<CounterExampleResults>
    {
        public string DafnyFile { get; set; }
    }
}
