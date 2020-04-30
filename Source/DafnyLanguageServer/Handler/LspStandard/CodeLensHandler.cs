using System;
using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.ProgramServices;
using DafnyServer;
using Microsoft.Dafny;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides CodeLens information for VS Code.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>CodeLensServiceProvider</c>.
    /// </summary>
    public class CodeLensHandler : LspBasicHandler<CodeLensCapability>, ICodeLensHandler
    {
        public CodeLensHandler(ILanguageServer router, WorkspaceManager workspaceManager, ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
        }

        public CodeLensRegistrationOptions GetRegistrationOptions()
        {
            return new CodeLensRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                ResolveProvider = false
            };
        }

        public async Task<CodeLensContainer> Handle(CodeLensParams request, CancellationToken cancellationToken)
        {

            _log.LogInformation("Handling Code Lens"); // todo lang file #102

            try
            {
                return await Task.Run(() =>
                {
                    List<CodeLens> items = new List<CodeLens>();
                    foreach (var modul in _workspaceManager?.SymbolTableManager?.SymbolTables)
                    {
                        foreach (var symbolInformation in modul.Value)
                        {

                            if ((symbolInformation.Kind == SymbolTable.Kind.Class || //todo ... not perfeect at all 
                                 symbolInformation.Kind == SymbolTable.Kind.Function ||
                                 symbolInformation.Kind == SymbolTable.Kind.Method) &&
                                // no constructors and make sure no out-of-range root _defaults
                                symbolInformation.Name != "_ctor" &&
                                symbolInformation?.Line != null && symbolInformation.Line > 0)
                            {
                                Position position = new Position((long)symbolInformation.Line - 1, 0);
                                Range range = new Range { Start = position, End = position };
                                Command command = new Command
                                {
                                    Title = (symbolInformation.Usages?.Count) + " reference(s) to " + // todo lang file #102
                                            symbolInformation.Name,
                                    Name = "dafny.showReferences",

                                    //Arguments = [filUri, rangeStaart, Locations] todo
                                    /*
                                     * editor.action.showReferences - Show references at a position in a file

uri - The text document in which to show references
position - The position at which to show
locations - An array of locations.
                                     */
                                };
                                items.Add(new CodeLens
                                { Data = request.TextDocument.Uri, Range = range, Command = command });
                            }
                        }
                    }
                    return new CodeLensContainer(items);
                });
            }

            catch (Exception e)
            {
                _log.LogError("Internal server error handling CodeLens: " + e.Message); // todo lang file #102
                new MessageSenderService(_router).SendError("Internal server error handling CodeLens: " + e.Message); // todo lang file #102
                return null; //todo warum return null... ght dat ned eleganter? sendError oder so via new throw ? #107
            }
        }
    }
}
