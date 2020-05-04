using System;
using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.ProgramServices;
using DafnyLanguageServer.SymbolTable;
using DafnyServer;
using Microsoft.Dafny;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides CodeLens information for VS Code.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>CodeLensServiceProvider</c>.
    /// </summary>
    public class CodeLensTaskHandler : LspBasicHandler<CodeLensCapability>, ICodeLensHandler
    {
        public CodeLensTaskHandler(ILanguageServer router, WorkspaceManager workspaceManager, ILoggerFactory loggingFactory)
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
                    var manager = _workspaceManager.SymbolTableManager;

                    foreach (var symbolInformation in manager.GetAllSymbolDeclarations())
                    {
                        items.Add(CreateCodeLensItem(symbolInformation, request));
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

        private CodeLens CreateCodeLensItem(ISymbol symbolInformation, CodeLensParams request)
        {
            Position position = new Position((long)symbolInformation.Line - 1, 0);
            Range range = new Range { Start = position, End = position };

            var locations = new List<Location>();
            if (symbolInformation.Usages.Any())
            {
                foreach (var usage in symbolInformation.Usages)
                {
                    var startPos = new Position
                    {
                        Line = usage.Position.BodyStartToken.line - 1,
                        Character = usage.Position.BodyStartToken.col
                    };
                    var endPos = new Position
                    {
                        Line = usage.Position.BodyEndToken.line - 1,
                        Character = usage.Position.BodyEndToken.col
                    };
                    locations.Add(new Location
                    {
                        Uri = request.TextDocument.Uri, // this wont work in references to other files... 
                        Range = new Range(startPos, endPos)
                    });
                }
            }

            // todo lang file #102
            // =?: operator
            string msgTitle;
            if (symbolInformation.Usages.Any())
            {
                if (symbolInformation.Usages.Count == 1)
                {
                    msgTitle = $"1 reference to {symbolInformation.Name}";
                }
                else
                {
                    msgTitle = $"{symbolInformation.Usages.Count} reference(s) to {symbolInformation.Name}";
                }
            }
            else
            {
                msgTitle = $"Never used yet. Can you remove {symbolInformation.Name}?";
            }

            var args = new
            {
                Uri = request.TextDocument.Uri,
                Position = position, // where to show the codelens popup
                Locations = locations // what should be in the pop up displayed 
            };
            Command command = new Command
            {
                Title = msgTitle,
                Name = "dafny.showReferences",
                Arguments = new JArray(JsonConvert.SerializeObject(args))
            };
            return new CodeLens
            {
                Data = request.TextDocument.Uri,
                Range = range,
                Command = command
            };
        }
    }
}
