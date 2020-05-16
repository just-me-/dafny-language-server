using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This handler implements a file watcher for Dafny files in the client workspace like it is defined in the LSP.
    /// This handler gets called qhenever a watched file gets changed from a third part program on the client side.
    /// We do actually not handle those requests separately.
    /// This watcher has been implemented to support the defined LSP and do not throw an "not implemented error" in case a Dafny file has been modified from another editor or program. 
    /// </summary>
    internal class DidChangeWatchedFilesHandler : IDidChangeWatchedFilesHandler
    {
        private DidChangeWatchedFilesCapability _capability;       //Omnisharp field, lets leave it in.

        public object GetRegistrationOptions()                     //Omnisharp "not yet implemented": https://github.com/OmniSharp/csharp-language-server-protocol/issues/197 #4234 should work now
        {
            return new DidChangeWatchedFilesRegistrationOptions
            {
                Watchers = new FileSystemWatcher[]
                {
                        new FileSystemWatcher{

                            GlobPattern = Resources.ConfigurationStrings.file_watch_pattern,
                            Kind = 7
                        }
                },
            };
        }

        public Task<Unit> Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public void SetCapability(DidChangeWatchedFilesCapability capability)
        {
            _capability = capability;
        }
    }

    internal class DidChangeWatchedFilesRegistrationOptions
    {
        public FileSystemWatcher[] Watchers { get; set; }
    }

    internal class FileSystemWatcher
    {
        public string GlobPattern { get; set; }
        public int Kind { get; set; }
    }
}