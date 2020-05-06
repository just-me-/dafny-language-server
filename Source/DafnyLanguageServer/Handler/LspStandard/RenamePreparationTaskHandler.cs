using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.FileManager;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DafnyLanguageServer.Handler.LspStandard
{
    public class RenamePreparationTaskHandler : LspBasicHandler<RenameCapability>, IPrepareRenameHandler
    {

        public RenamePreparationTaskHandler(ILanguageServer router, WorkspaceManager workspaceManager, ILoggerFactory loggingFactory = null)
            : base(router, workspaceManager, loggingFactory)
        {
        }

        public object GetRegistrationOptions() //todo richtig=? was muss ich hier machen? ... mal sehen obs tut xD
        {
            return new TextDocumentRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }

        public Task<RangeOrPlaceholderRange> Handle(PrepareRenameParams request, CancellationToken cancellationToken)
        {
            try
            {
                return Task.Run(() =>
                {
                    var file = _workspaceManager.GetFileRepository(request.TextDocument.Uri);
                    var stMan = _workspaceManager.SymbolTableManager;
                    var line = (int)request.Position.Line + 1;
                    var col = (int)request.Position.Character + 1;
                    var symbolAtCursor = stMan.GetSymbolByPosition(line, col);

                    if (symbolAtCursor == null)
                    {
                        return null;
                    }

                    var range = new Range()
                    {
                        Start = new Position(symbolAtCursor.Line-1 ?? 0, symbolAtCursor.Column-1 ?? 0),
                        End = new Position(symbolAtCursor.Line-1 ?? 0, symbolAtCursor.ColumnEnd-1 ?? 0)
                    };
                    var result = new RangeOrPlaceholderRange(range); //?? richitg?? was mach ich hier? wtf ist ein placeholderrange?

                    return result;
                });
            } catch (Exception e)
            {
                _log.LogError("Error Handling Rename Preparation: " + e.Message);
                return null; 
            }
        }
    }
}
