using System;
using System.Collections.Generic;
using System.Text;

namespace DafnyLanguageServer.SymbolTable
{
    public class SymbolTableManager : ISymbolTableManager
    {
        public ISymbol DafnyProgramRootSymbol { get; }

        public SymbolTableManager(ISymbol root) => DafnyProgramRootSymbol = root;

        public ISymbol GetSymbolByPosition(Uri file, int line, int character)
        {
            INavigator navigator = new SymbolTableNavigator();
            var symbol = navigator.GetSymbolByPosition(DafnyProgramRootSymbol, file, line, character);
            return symbol;
        }

        /// <summary>
        /// In case the user is typing and there can not be an entry point via a Symbol
        /// (eg for auto completion), the entry point has to be via the scope of the wrapping parent symbol.
        /// Use this method to get the parent symbol as en entry point.
        /// </summary>
        //  weg
        public ISymbol GetSymbolWrapperForCurrentScope(Uri file, int line, int character)
        {
            ISymbol closestWrappingSymbol = null;
            INavigator navigator = new SymbolTableNavigator();
            closestWrappingSymbol = navigator.TopDown(DafnyProgramRootSymbol, file, line, character);

            if (closestWrappingSymbol == null && DafnyProgramRootSymbol[Resources.SymbolTableStrings.default_module] != null)
            {
                return DafnyProgramRootSymbol[Resources.SymbolTableStrings.default_module];
            }
            return closestWrappingSymbol;
        }

        /// <summary>
        /// Provide an entry point (symbol) and a string (name of a symbol) you are looking for.
        /// This method returns the nearest declaration with that name that can be found.
        /// </summary>
        public ISymbol GetClosestSymbolByName(ISymbol entryPoint, string symbolName)
        {
            INavigator navigator = new SymbolTableNavigator();
            bool filter(ISymbol x) => x.IsDeclaration && x.Name == symbolName;
            return navigator.BottomUpFirst(entryPoint, filter);
        }

        /// <summary>
        /// This returns all symbol declaration that are in scope for the given symbol.
        /// This recursive and can be used for functions like auto completion.
        /// </summary>
        public List<ISymbol> GetAllDeclarationForSymbolInScope(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            filter = filter ?? (x => true);
            INavigator navigator = new SymbolTableNavigator();
            bool extendedFilter(ISymbol x) => x.IsDeclaration && x.Kind != Kind.Constructor && filter.Invoke(x);
            return navigator.BottomUpAll(symbol, extendedFilter);
        }

        /// <summary>
        /// Return the declaration of a symbol or itself.
        /// Used for Go2Definition.
        /// </summary>
        public ISymbol GetOriginFromSymbol(ISymbol symbol)  //todo  direkt symbol.Origin nutzen
        {
            return symbol.DeclarationOrigin;
        }

        /// <summary>
        /// For instances of classes - returns the origins "class type" as Smybol.
        /// Eg var instance = new ClassA();
        /// Calling this function with instance will return the symbol of ClassA (origin).
        /// </summary>
        public ISymbol GetClassOriginFromSymbol(ISymbol symbol)
        {
            // todo anders machen... GetClassSymbolByPath is ansich nicht richtig. 
            var classPath = GetOriginFromSymbol(symbol).UserTypeDefinition.ResolvedClass.FullName;
            return GetClassSymbolByPath(classPath);
        }

        private ISymbol GetClassSymbolByPath(string classPath)
        {
            // todo better do a split? is there rly every time "just one module and one class"? #212 | das kann auf jedenfall net richtig sien. Wa sbie Modul1.Modul2.Modul3.Class?
            // ev siehe zteile +/- 70
            string[] originPath = classPath.Split('.'); // 0 = module, 1 = class
            if (originPath.Length != 2)
            {
                throw new ArgumentException(Resources.ExceptionMessages.tmp_invalid_class_path);
            }
            return DafnyProgramRootSymbol[originPath[0]][originPath[1]];
        }

        /// <summary>
        /// Gets all Symbol declarations.
        /// this can for example be used to know where to show CodeLens information.
        /// </summary>
        public List<ISymbol> GetAllSymbolDeclarations()
        {
            List<ISymbol> symbols = new List<ISymbol>();
            INavigator navigator = new SymbolTableNavigator();

            bool filter(ISymbol symbol) =>
                symbol.IsDeclaration && (symbol.Kind == Kind.Class || symbol.Kind == Kind.Function || symbol.Kind == Kind.Method && symbol.Name != Resources.SymbolTableStrings.dafnys_entry_point) &&
                // no constructors and make sure no out-of-range root _defaults
                symbol.Kind != Kind.Constructor && symbol.Line > 0;

            symbols.AddRange(navigator.TopDownAll(DafnyProgramRootSymbol, filter));

            return symbols;
        }

        /// <summary>
        /// Creates a string representation of the symbol tree.
        /// </summary>
        /// <returns>Returns a string representation of the symbol table attached to this manager.</returns>
        public string CreateDebugReadOut()
        {
            StringBuilder b = new StringBuilder();
            INavigator nav = new SymbolTableNavigator();
            var rootSymbol = DafnyProgramRootSymbol;

            var allSymbs = nav.TopDownAll(rootSymbol);

            foreach (var symbol in allSymbs)
            {
                b.AppendLine(symbol.ToDebugString());
            }

            b.AppendLine();

            return b.ToString();
        }
    }
}
