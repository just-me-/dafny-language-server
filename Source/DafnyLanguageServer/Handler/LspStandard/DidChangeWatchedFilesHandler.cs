﻿using DafnyLanguageServer.WorkspaceManager;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
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
    internal class DidChangeWatchedFilesHandler : LspBasicHandler<DidChangeWatchedFilesCapability>, IDidChangeWatchedFilesHandler
    {
        public DidChangeWatchedFilesHandler(ILanguageServer router, IWorkspace workspaceManager, ILoggerFactory loggingFactory = null)
            : base(router, workspaceManager, loggingFactory)
        {
            _method = Resources.Requests.watchedFileChanged;
        }

        public object GetRegistrationOptions()
        {
            return new DidChangeWatchedFilesRegistrationOptions
            {
                Watchers = new[]
                {
                    new FileSystemWatcher{
                        GlobPattern = Resources.ConfigurationStrings.file_watch_pattern,
                        Kind = 7
                    }
                }
            };
        }

        public Task<Unit> Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation(string.Format(Resources.LoggingMessages.request_handle, _method));
            return Unit.Task;
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