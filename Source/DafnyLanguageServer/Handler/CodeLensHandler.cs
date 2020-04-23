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
        private CodeLensCapability _capability;

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
<<<<<<< HEAD
            _log.LogInformation("Handling Code Lens");

            try
            { //2do
            return await Task.Run(() =>
            {
                List<CodeLens> items = new List<CodeLens>();
                foreach (var modul in _workspaceManager?.SymbolTableManager?.SymbolTables)
                {
                    foreach (var symbolInformation in modul.Value)
                    {
                        if ((symbolInformation.Type == SymbolTable.Type.Class ||
                             symbolInformation.Type == SymbolTable.Type.Function ||
                             symbolInformation.Type == SymbolTable.Type.Method) &&
                            // no constructors and make sure no out-of-range root _defaults
                            symbolInformation.Name != "_ctor" &&
                            symbolInformation?.Line != null && symbolInformation.Line > 0)
                        {
                            Position position = new Position((long) symbolInformation.Line - 1, 0);
                            Range range = new Range {Start = position, End = position};
                            Command command = new Command
                            {
                                Title = (symbolInformation.Usages?.Count) + " reference(s) to " +
                                        symbolInformation.Name,
                                Name = "dafny.showReferences"
                            };
                            items.Add(new CodeLens
                                {Data = request.TextDocument.Uri, Range = range, Command = command});
                        }
                    }

                    return new CodeLensContainer(items);
                });
            }
            catch (Exception e)
            {
                _log.LogError("Internal server error handling CodeLens: " + e.Message);
                new MessageSenderService(_router).SendError("Internal server error handling CodeLens: " + e.Message);
                return null;
            }
        }
    }
}
