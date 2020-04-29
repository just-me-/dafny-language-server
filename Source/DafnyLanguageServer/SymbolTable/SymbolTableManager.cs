using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// Provides all needed SymbolTableInformation for all Modules. Needs a Dafny Program to work. 
    /// <c>SymbolTables</c> is a key-value-hash. Key is the module name (string) and value is a set of <c>SymbolInformation.</c>
    /// A SymbolTable (List of <c>SymbolInformation</c> for each module) is sorted.
    /// Mostly there is only one module - the default module for a single Dafny file. 
    /// </summary>
    public class SymbolTableManager
    {
        private readonly Microsoft.Dafny.Program _dafnyProgram;
        public Dictionary<string, List<SymbolInformation>> SymbolTables { get; set; } = new Dictionary<string, List<SymbolInformation>>();  //still nicht sicher ob das sinnvoll ist.... eig könnt man auch alles in eine einzige tabelle aggregieren mit allen modulen (?)

        public SymbolTableManager(Microsoft.Dafny.Program dafnyProgram)
        {
            _dafnyProgram = dafnyProgram;
            GenerateSymbolTable();
        }

        private void GenerateSymbolTable()
        {
            foreach (var module in _dafnyProgram.Modules())
            {
                var declarationVisitor = new LanguageServerDeclarationVisitor();
                module.Accept(declarationVisitor);
                var declarationTable = declarationVisitor.SymbolTable;

                var deepVisitor = new SymbolTableVisitorEverythingButDeclarations();
                deepVisitor.SymbolTable = declarationTable;

                module.Accept(deepVisitor);

                SymbolTables.Add(module.Name, deepVisitor.SymbolTable);

                string debugMe = CreateDebugReadOut();
            }
        }

        public string CreateDebugReadOut()
        {
            StringBuilder b = new StringBuilder();
            foreach (var kvp in SymbolTables)
            {
                b.AppendLine("Module: " + kvp.Key);
                foreach (var symbol in kvp.Value)
                {
                    b.AppendLine(symbol.ToString());
                }
            }
            return b.ToString();
        }

        public List<string> GetEntriesAsStringList()
        {
            return (from kvp in SymbolTables from symbol in kvp.Value select symbol.ToString()).ToList();
        }

        public SymbolInformation GetSymbolByPosition(int line, int character)
        {
            var tmpDebugList = new List<SymbolInformation>();
            foreach (var modul in SymbolTables)
            {
                foreach (var symbolInformation in modul.Value)
                {
                    if (PositionIsInSymbolsRange(line, character, symbolInformation))
                        tmpDebugList.Add(symbolInformation);
                }
            }
            return tmpDebugList.Count > 0 ? tmpDebugList[0] : null;
        }

        private SymbolInformation GetClassSymbolByPath(string classPath)
        {
            // todo better do a split? is there rly every time "just one module and one class"? #212
            string[] originPath = classPath.Split('.'); // 0 = module, 1 = class
            if (originPath.Length != 2)
            {
                throw new ArgumentException("Invalid class path... expected Module.Class pattern."); //tmp
            }
            // a hash in the hash would be more efficient... composit pattern? todo 
            return SymbolTables[originPath[0]]?.Find(symbol => symbol.Name == originPath[1] && symbol.IsDeclaration);
        }

        private bool PositionIsInSymbolsRange(int line, int character, SymbolInformation symbol)
        {
            return (symbol.Line <= line
                    && symbol.Line >= line
                    && symbol.ColumnStart <= character
                    && symbol.ColumnEnd >= character);
        }

        /// <summary>
        /// In case the user is typing and there can not be an entry point via a Symbol
        /// (eg for auto completion), the entry point has to be via the scope of the wrapping parent symbol.
        /// Use this method to get the parent symbol as en entry point. 
        /// </summary>
        public SymbolInformation GetSymbolWrapperForCurrentScope(int line, int character)
        {
            SymbolInformation closestWrappingSymbol = null;
            int smallestSpanOfWrappingSymbol = -1;
            foreach (var modul in SymbolTables)
            {
                foreach (var symbolInformation in modul.Value)
                {
                    if (SymbolWrapsPosition(line, character, symbolInformation))
                    {
                        var span = CalculateSpanOfSymbol(symbolInformation);
                        if ((closestWrappingSymbol == null) // for the first round 
                            || (span >= 0 && span <= smallestSpanOfWrappingSymbol))
                        {
                            smallestSpanOfWrappingSymbol = span;
                            closestWrappingSymbol = symbolInformation;
                        }
                    }
                }
            }
            return closestWrappingSymbol;
        }

        private bool SymbolWrapsPosition(int line, int character, SymbolInformation symbol)
        {
            var symbolStartLine = symbol?.Position?.BodyStartToken?.line;
            var symbolEndLine = symbol?.Position?.BodyEndToken?.line;

            var symbolStartChar = symbol?.Position?.BodyStartToken?.col;
            var symbolEndChar = symbol?.Position?.BodyEndToken?.col;

            return (symbolStartLine != null && symbolEndLine != null)
                   &&
                   (
                    (symbolStartLine <= line
                    && symbolEndLine >= line
                    && symbolStartLine != symbolEndLine)
                   || // if it is on one line - check position 
                    (symbolStartLine == symbolEndLine
                    && symbolStartLine == line
                    && symbolStartChar <= character
                    && symbolEndChar >= character)
                   );
        }

        /// <summary>
        /// returns -1 in case of an error 
        /// </summary>
        private int CalculateSpanOfSymbol(SymbolInformation symbol)
        {
            var symbolStartLine = symbol?.Position?.BodyStartToken?.line;
            var symbolEndLine = symbol?.Position?.BodyEndToken?.line;
            return (symbolEndLine != null && symbolStartLine != null)
                ? ((int)symbolEndLine - (int)symbolStartLine)
                : -1;
        }

        /// <summary>
        /// Provide an entry point (symbol) and a string (name of a symbol) you are looking for.
        /// This method returns the nearest declaration with that name that can be found. 
        /// </summary>
        public SymbolInformation GetClosestSymbolByName(SymbolInformation entryPoint, string symbolName)
        {
            var matchingSymbol = GetSpecificChild(entryPoint, symbolName);
            if (matchingSymbol != null)
            {
                return matchingSymbol;
            }

            var parent = entryPoint.Parent;
            while (parent != null)
            {
                matchingSymbol = GetSpecificChild(parent, symbolName);
                if (matchingSymbol != null)
                {
                    return matchingSymbol;
                }
                parent = parent.Parent;
            }
            return null;
        }

        private SymbolInformation GetSpecificChild(SymbolInformation symbol, string symbolName)
        {
            if (symbol == null || symbol.Children == null)
            {
                return null;
            }
            foreach (var childSymbol in symbol.Children)
            {
                if (childSymbol.IsDeclaration && childSymbol.Name == symbolName)
                {
                    return childSymbol;
                }
            }
            return null;
        }

        /// <summary>
        /// This returns all symbol declaration that are in scope for the given symbol.
        /// This recursive and can be used for functions like auto completion. 
        /// </summary>
        public List<SymbolInformation> GetAllDeclarationForSymbolInScope(SymbolInformation symbol)
        {
            List<SymbolInformation> list = new List<SymbolInformation>();
            list.AddRange(GetAllChilds(symbol, null));

            var parent = symbol.Parent;
            while (parent != null)
            {
                list.AddRange(GetAllChilds(parent, symbol));
                parent = parent.Parent;
            }

            // erst durch alle childs.. und childs von childs... 
            // und alle parents dann... uund bei denen auch wieder tief runter etc. 

            // ---- njääääh? die sind ja eig alle "nicht bekannt"... an der aktuellen "position" 

            return list;
        }

        private List<SymbolInformation> GetAllChilds(SymbolInformation symbol, SymbolInformation excludeSymbol)
        {
            if (symbol == null || symbol.Children == null)
            {
                return null;
            }
            List<SymbolInformation> list = new List<SymbolInformation>();
            foreach (var childSymbol in symbol.Children)
            {
                if (childSymbol.IsDeclaration
                    && (excludeSymbol == null || excludeSymbol.Name != childSymbol.Name)
                    && childSymbol.Name != "_ctor"
                    && childSymbol.Name != "_default")// effizienter machbar? 
                {
                    list.Add(childSymbol);
                }
                // njäääh - childs von childs sind ja eigentlich unbekannt
                // an der aktuellen position 
                // list.AddRange(GetAllChilds(childSymbol, excludeSymbol));
            }
            return list;
        }

        /// <summary>
        /// Return itself if it is already a declaration.
        /// Used for Go2Definition. 
        /// </summary>
        public SymbolInformation GetOriginFromSymbol(SymbolInformation symbol)
        {
            return symbol.DeclarationOrigin;
        }

        /// <summary>
        /// For instances of classes - returns the origins "class type" as Smybol.
        /// Eg var instance = new ClassA();
        /// Calling this function with instance will return the symbol of ClassA (origin). 
        /// </summary>
        public SymbolInformation GetClassOriginFromSymbol(SymbolInformation symbol)
        {
            var classPath = GetOriginFromSymbol(symbol).UserTypeDefinition.ResolvedClass.FullName;
            return GetClassSymbolByPath(classPath);
        }

        // CodeLens
        public List<SymbolInformation> GetUsagesOfSymbol(SymbolInformation symbol)
        {
            return null;
        }
    }
}
