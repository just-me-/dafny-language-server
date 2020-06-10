using MediatR;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Threading;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// Handels the request "Shutdown" and "Exit".
    /// </summary>
    public class ShutdownHandler : IShutdownHandler, IExitHandler
    {
        public Task<Unit> Handle(EmptyRequest request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }
    }
}