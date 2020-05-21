namespace DafnyLanguageServer.SymbolTable
{
    public interface ISymbolTableGenerator
    {
        ISymbol GenerateSymbolTable();
    }
}
