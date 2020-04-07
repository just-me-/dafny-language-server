using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This component provides virtual methods for Handler components that are basic LSP standard features.
    /// </summary>
    public class LspBasicHandler
    {
        protected DefinitionCapability _capability;
        protected readonly ILanguageServer _router;
        protected readonly WorkspaceManager _workspaceManager;

        public LspBasicHandler(ILanguageServer router, WorkspaceManager workspaceManager)
        {
            _router = router;
            _workspaceManager = workspaceManager;
        }


        public virtual TextDocumentRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions
            {
                DocumentSelector = new DocumentSelector(
                    new DocumentFilter()
                    {
                        Pattern = "**/*.dfy"
                    })
            };
        }

        public virtual void SetCapability(DefinitionCapability capability)
        {
            _capability = capability;
        }
    }
}
