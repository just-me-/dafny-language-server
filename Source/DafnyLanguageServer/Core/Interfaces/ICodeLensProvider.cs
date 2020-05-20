using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    public interface ICodeLensProvider
    {
        CodeLensContainer GetCodeLensContainer();
    }
}
