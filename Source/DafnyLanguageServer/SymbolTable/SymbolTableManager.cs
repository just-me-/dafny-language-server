using System;
using System.Collections.Generic;
using System.Text;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// Use this class to navigate on the symbol table.
    /// It handles access based on a root symbol.
    /// Navigation can be done by using predicates, such as "only declarations".
    /// </summary>
    public class SymbolTableManager : ISymbolTableManager
    {
        public ISymbolInformation DafnyProgramRootSymbol { get; }

        public SymbolTableManager(ISymbolInformation root) => DafnyProgramRootSymbol = root;

        public ISymbolInformation GetSymbolByPosition(Uri file, int line, int character)
        {
            ISymbolNavigator navigator = new SymbolNavigator();
            var symbol = navigator.GetSymbolAtPosition(DafnyProgramRootSymbol, file, line, character);
            return symbol;
        }

        /// <summary>
        /// In case the user is typing and there can not be an entry point via a symbol
        /// (eg for auto completion), the entry point has to be via the scope of the wrapping parent symbol.
        /// Use this method to get the parent symbol as en entry point.
        /// </summary>
        public ISymbolInformation GetSymbolWrapperForCurrentScope(Uri file, int line, int character)
        {
            ISymbolNavigator navigator = new SymbolNavigator();
            var closestWrappingSymbol = navigator.TopDown(DafnyProgramRootSymbol, file, line, character);

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
        public ISymbolInformation GetClosestSymbolByName(ISymbolInformation entryPoint, string symbolName)
        {
            ISymbolNavigator navigator = new SymbolNavigator();
            bool filter(ISymbolInformation x) => x.IsDeclaration && x.Name == symbolName;
            return navigator.BottomUpFirst(entryPoint, filter);
        }

        /// <summary>
        /// This returns all symbol declaration that are in scope for the given symbol.
        /// This recursive and can be used for functions like auto completion.
        /// </summary>
        public List<ISymbolInformation> GetAllDeclarationForSymbolInScope(ISymbolInformation symbol, Predicate<ISymbolInformation> filter = null)
        {
            filter = filter ?? (x => true);
            ISymbolNavigator navigator = new SymbolNavigator();
            bool extendedFilter(ISymbolInformation x) => x.IsDeclaration && x.Kind != Kind.Constructor && filter.Invoke(x);
            return navigator.BottomUpAll(symbol, extendedFilter);
        }

        /// <summary>
        /// For instances of classes - returns the origins "class type" as symbol.
        /// Eg var instance = new ClassA();
        /// Calling this function with instance will return the symbol of ClassA (origin).
        /// </summary>
        public ISymbolInformation GetClassOriginFromSymbol(ISymbolInformation symbol)
        {
            string classPath;
            try
            {
                classPath = symbol.DeclarationOrigin.UserTypeDefinition.ResolvedClass.FullName;
            }
            catch (NullReferenceException)
            {
                return null;
            }

            string[] originPath = classPath.Split('.');
            LinkedList<string> chain = new LinkedList<string>(originPath);

            var result = DafnyProgramRootSymbol;
            while (chain.Count != 0)
            {
                result = result[chain.First.Value];
                chain.RemoveFirst();
            }

            return result;
        }

        /// <summary>
        /// Gets all symbol declarations.
        /// this can for example be used to know where to show CodeLens information.
        /// </summary>
        public List<ISymbolInformation> GetAllSymbolDeclarations()
        {
            List<ISymbolInformation> symbols = new List<ISymbolInformation>();
            ISymbolNavigator navigator = new SymbolNavigator();

            bool filter(ISymbolInformation symbol) =>
                symbol.IsDeclaration && (symbol.Kind == Kind.Class || symbol.Kind == Kind.Function || symbol.Kind == Kind.Method && symbol.Name != Resources.SymbolTableStrings.dafnys_entry_point) &&
                // no constructors and make sure no out-of-range root _defaults
                symbol.Kind != Kind.Constructor && symbol.Line > 0;

            symbols.AddRange(navigator.TopDownAll(DafnyProgramRootSymbol, filter));

            return symbols;
        }

        /// <summary>
        /// Returns the class in which a symbol is located.
        /// </summary>
        public ISymbolInformation GetEnclosingClass(ISymbolInformation entryPoint)
        {
            ISymbolNavigator navigator = new SymbolNavigator();
            bool filter(ISymbolInformation x) => x.IsDeclaration && x.Kind == Kind.Class;
            return navigator.BottomUpFirst(entryPoint, filter);
        }

        /// <summary>
        /// Creates a string representation of the symbol tree.
        /// </summary>
        /// <returns>Returns a string representation of the symbol table attached to this manager.</returns>
        public string CreateDebugReadOut()
        {
            StringBuilder b = new StringBuilder();
            ISymbolNavigator nav = new SymbolNavigator();
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