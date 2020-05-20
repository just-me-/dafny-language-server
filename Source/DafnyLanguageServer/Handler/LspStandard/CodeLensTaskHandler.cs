using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides CodeLens information for VS Code.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>CodeLensServiceProvider</c>.
    /// </summary>
    public class CodeLensTaskHandler : LspBasicHandler<CodeLensCapability>, ICodeLensHandler
    {
        public CodeLensTaskHandler(ILanguageServer router, IWorkspace workspaceManager, ILoggerFactory loggingFactory)
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
                var manager = _workspaceManager.SymbolTableManager;
                var uri = request.TextDocument.Uri;
                ICodeLensProvider provider = new CodeLensProvider(manager, uri);
                return await Task.Run(() => provider.GetCodeLensContainer(), cancellationToken);
            }
            catch (Exception e)
            {
                HandleError("Error while processing code lens request", e); //todo lang
                return null;
            }
        }
    }
}
