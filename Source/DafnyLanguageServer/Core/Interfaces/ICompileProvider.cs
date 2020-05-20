using DafnyLanguageServer.Handler;

namespace DafnyLanguageServer.Core
{
    public interface ICompileProvider
    {
        CompilerResults Compile();
    }
}
