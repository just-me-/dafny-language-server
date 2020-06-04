namespace DafnyLanguageServer.SymbolTable
{
    public interface ISymbolTableGenerator
    {
        ISymbolInformation GenerateSymbolTable();
    }
}
