﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.SymbolTable;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Handler.LspStandard
{
    public class RenameTaskHandler : LspBasicHandler<RenameCapability>, IRenameHandler
    {
        public RenameTaskHandler(ILanguageServer router, WorkspaceManager workspaceManager,
            ILoggerFactory loggingFactory = null)
            : base(router, workspaceManager, loggingFactory)
        {
        }


        public RenameRegistrationOptions GetRegistrationOptions()
        {
            return new RenameRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                PrepareProvider = true
            };
        }

        public Task<WorkspaceEdit> Handle(RenameParams request, CancellationToken cancellationToken)
        {
            try
            {
                return Task.Run(() =>
                {
                    //todo das komisch... müsste ich nicht iwie noch das file angeben, hust hust, wtf xD

                    var file = _workspaceManager.GetFileRepository(request.TextDocument.Uri);
                    var stMan = _workspaceManager.SymbolTableManager;
                    var line = (int)request.Position.Line + 1;
                    var col = (int)request.Position.Character + 1;
                    var symbolAtCursor = stMan.GetSymbolByPosition(line, col); 
                    IEnumerable<ISymbol> symbolsToRename = stMan.GetAllOccurences(symbolAtCursor);
                    List<TextEdit> editsForOneFile = new List<TextEdit>(); //todo multifile, import, blabla

                    foreach (var symbol in symbolsToRename)
                    {
                        var textEdit = new TextEdit
                        {
                            NewText = request.NewName,
                            Range = new Range()
                            {
                                Start = new Position(symbol.Line-1 ?? 0, symbol.Column-1 ?? 0),
                                End = new Position(symbol.Line-1 ?? 0, symbol.ColumnEnd-1 ?? 0)
                            }
                        };
                        editsForOneFile.Add(textEdit);
                    }

                    var result = new WorkspaceEdit
                    {
                        Changes = new Dictionary<Uri, IEnumerable<TextEdit>>
                        {
                            {request.TextDocument.Uri, editsForOneFile}
                        }
                    };
                    return result;
                });
            }
            catch (Exception e)
            {
                _log.LogError("Error Handling Rename Execution: " + e.Message);
                return null;
            }
        }

    }
}