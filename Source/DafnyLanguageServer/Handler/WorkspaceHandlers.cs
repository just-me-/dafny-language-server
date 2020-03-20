using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.Services;
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

    internal class DidChangeWatchedFilesHandler : IDidChangeWatchedFilesHandler
    {
        private DidChangeWatchedFilesCapability _capability;       //Omnisharp field, lets leave it in.

        public object GetRegistrationOptions()                     //Omnisharp "not yet implemented": https://github.com/OmniSharp/csharp-language-server-protocol/issues/197
        {
            return new DidChangeWatchedFilesRegistrationOptions
            {
                Watchers = new FileSystemWatcher[]
                {
                        new FileSystemWatcher{
                            GlobPattern = "**/*.dfy",
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