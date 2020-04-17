using OmniSharp.Extensions.JsonRpc;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.HandlerServices;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// Handels the request "Shutdown" and "Exit".
    /// Does nothing, but was meant to prevent exceptions:
    /// Failed to handle request shutdown
    /// System.MissingMethodException: Für dieses Objekt wurde kein parameterloser Konstruktor definiert.
    /// bei System.RuntimeTypeHandle.CreateInstance(RuntimeType type, Boolean publicOnly, Boolean noCheck, Boolean&amp; canBeCached, RuntimeMethodHandleInternal&amp; ctor, Boolean&amp; bNeedSecurityCheck)
    /// bei System.RuntimeType.CreateInstanceSlow(Boolean publicOnly, Boolean skipCheckThis, Boolean fillCache, StackCrawlMark&amp; stackMark)
    /// bei System.RuntimeType.CreateInstanceDefaultCtor(Boolean publicOnly, Boolean skipCheckThis, Boolean fillCache, StackCrawlMark&amp; stackMark)
    /// bei System.Activator.CreateInstance(Type type, Boolean nonPublic)
    /// bei System.Activator.CreateInstance(Type type)
    /// </summary>
    public class ShutdownHandler : IShutdownHandler, IExitHandler
    {
        public Task<Unit> Handle(EmptyRequest request, CancellationToken cancellationToken)
        {
            
            return Unit.Task;
        }
    }
}
