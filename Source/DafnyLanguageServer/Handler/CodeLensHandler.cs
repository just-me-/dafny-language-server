using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.DafnyAccess;
using DafnyServer;
using Microsoft.Dafny;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides CodeLens information for VS Code.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>CodeLensServiceProvider</c>. 
    /// </summary>
    public class CodeLensHandler : LspBasicHandler<CodeLensCapability>, ICodeLensHandler
    {
        public CodeLensHandler(ILanguageServer router, WorkspaceManager workspaceManager)
            : base(router, workspaceManager)
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
            return await Task.Run(() =>
            {
                List<CodeLens> items = new List<CodeLens>();

                if (_workspaceManager != null && _workspaceManager.SymbolTableManager != null &&
                    _workspaceManager.SymbolTableManager.SymbolTables != null)
                {

                    foreach (var modul in _workspaceManager.SymbolTableManager.SymbolTables)
                    {
                        foreach (var symbolInformation in modul.Value)
                        {
                            if ((symbolInformation.Type == SymbolTable.Type.Class ||
                                 symbolInformation.Type == SymbolTable.Type.Function ||
                                 symbolInformation.Type == SymbolTable.Type.Method) &&
                                // no constructors and make sure no out-of-range root _defaults
                                symbolInformation.Name != "_ctor" &&
                                symbolInformation?.LineStart != null && symbolInformation.LineStart > 0)
                            {
                                Position position = new Position((long) symbolInformation.LineStart - 1, 0);
                                Range range = new Range {Start = position, End = position};
                                Command command = new Command
                                {
                                    Title = ((symbolInformation.Usages?.Count)+0) + " reference(s) to " +
                                            symbolInformation.Name,
                                    Name = "dafny.showReferences"
                                };
                                items.Add(new CodeLens
                                    {Data = request.TextDocument.Uri, Range = range, Command = command});
                            }
                        }
                    }
                    return new CodeLensContainer(items);
                }
                else
                {
                    return new CodeLensContainer();
                }
            });
        }
    }
}
