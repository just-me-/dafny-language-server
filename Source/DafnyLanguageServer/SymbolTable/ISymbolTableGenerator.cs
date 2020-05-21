using System;
using System.Collections.Generic;

namespace DafnyLanguageServer.SymbolTable
{
    public interface ISymbolTableGenerator
    {
        ISymbol GenerateSymbolTable();
    }
}
