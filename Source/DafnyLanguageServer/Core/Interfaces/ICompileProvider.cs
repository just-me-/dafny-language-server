using DafnyLanguageServer.CustomDTOs;

namespace DafnyLanguageServer.Core
{
    public interface ICompileProvider
    {
        CompilerResults Compile();
    }
}