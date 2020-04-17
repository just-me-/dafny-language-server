using DafnyLanguageServer.FileManager;
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
    public class CodeLensHandler : LspBasicHandler, ICodeLensHandler
    {
        private CodeLensCapability _capability;

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
        public void SetCapability(CodeLensCapability capability)
        {
            _capability = capability;
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
                    if (symbol.SymbolType == DafnyServer.OldSymbolTable.OldSymbolInformation.Type.Class ||
                        symbol.SymbolType == DafnyServer.OldSymbolTable.OldSymbolInformation.Type.Function ||
                        symbol.SymbolType == DafnyServer.OldSymbolTable.OldSymbolInformation.Type.Method)
                    {
                        var symbolReferencecounter = symbol.SymbolType == DafnyServer.OldSymbolTable.OldSymbolInformation.Type.Class ? 1 : 0;
                        foreach (var fileBuffers in _workspaceManager.GetAllFiles().Values)
                        {
                            foreach (var filesSymboltable in fileBuffers.SymboleProcessor().GetFullList())
                            {
                                if (symbol.SymbolType == DafnyServer.OldSymbolTable.OldSymbolInformation.Type.Class)
                                {
                                    // not working well yet - ticket #40
                                    if (filesSymboltable.ParentClass == symbol.Name
                                        && filesSymboltable.SymbolType == DafnyServer.OldSymbolTable.OldSymbolInformation.Type.Definition
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
    }
}
