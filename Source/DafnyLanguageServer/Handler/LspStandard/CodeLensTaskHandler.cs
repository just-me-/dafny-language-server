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
using DafnyLanguageServer.SymbolTable;
using DafnyServer;
using Microsoft.Dafny;
using Microsoft.Extensions.Logging;

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
            Command command = new Command
            {
                Title = (symbolInformation.Usages?.Count) + " reference(s) to " + // todo lang file #102
                        symbolInformation.Name,
                Name = "dafny.showReferences",
            };
            return new CodeLens { Data = request.TextDocument.Uri, Range = range, Command = command };
        }
    }
}
