using DafnyLanguageServer.ContentManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.DafnyAccess;
using DafnyServer;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides CodeLens information for VS Code.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>CodeLensServiceProvider</c>. 
    /// </summary>
    public class CodeLensHandler : ICodeLensHandler
    {
        private CodeLensCapability _capability;
        private readonly ILanguageServer _router;
        private readonly WorkspaceManager _workspaceManager;

        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.dfy"
            }
        );

        public CodeLensHandler(ILanguageServer router, WorkspaceManager workspaceManager)
        {
            _router = router;
            _workspaceManager = workspaceManager;
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

                var fileSymboltable = _workspaceManager.GetFileRepository(request.TextDocument.Uri).SymboleProcessor();
                if (fileSymboltable is null)
                {
                    return new CodeLensContainer();
                }
                foreach (var symbol in fileSymboltable.GetFullList())
                {
                    if (symbol.SymbolType == SymbolTable.SymbolInformation.Type.Class ||
                        symbol.SymbolType == SymbolTable.SymbolInformation.Type.Function ||
                        symbol.SymbolType == SymbolTable.SymbolInformation.Type.Method)
                    {
                        var symbolReferencecounter = symbol.SymbolType == SymbolTable.SymbolInformation.Type.Class ? 1 : 0;
                        foreach (var fileBuffers in _workspaceManager.GetAllFiles().Values)
                        {
                            foreach (var filesSymboltable in fileBuffers.SymboleProcessor().GetFullList())
                            {
                                if (symbol.SymbolType == SymbolTable.SymbolInformation.Type.Class)
                                {
                                    // not working well yet - ticket #40
                                    if (filesSymboltable.ParentClass == symbol.Name
                                        && filesSymboltable.SymbolType == SymbolTable.SymbolInformation.Type.Definition
                                        && filesSymboltable.Module is null)
                                        symbolReferencecounter++;
                                }
                                else
                                {
                                    if (filesSymboltable.Name == symbol.Name)
                                        symbolReferencecounter++;
                                }
                            }
                        }
                        Position position = new Position((long)symbol.Line - 1, 0);
                        Range range = new Range { Start = position, End = position };
                        Command command = new Command
                        {
                            Title = (symbolReferencecounter - 1) + " reference(s) to " + symbol.Name,
                            Name = "dafny.showReferences"
                        };
                        items.Add(new CodeLens { Data = request.TextDocument.Uri, Range = range, Command = command });
                    }
                }
                return new CodeLensContainer(items);
            });
        }

        public void SetCapability(CodeLensCapability capability)
        {
            _capability = capability;
        }
    }
}
