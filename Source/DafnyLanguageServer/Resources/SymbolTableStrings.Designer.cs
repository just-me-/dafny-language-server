﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DafnyLanguageServer.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SymbolTableStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SymbolTableStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DafnyLanguageServer.Resources.SymbolTableStrings", typeof(SymbolTableStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Main.
        /// </summary>
        internal static string dafnys_entry_point {
            get {
                return ResourceManager.GetString("dafnys_entry_point", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to *ERROR - DECLARATION SYMBOL NOT FOUND*.
        /// </summary>
        internal static string declaration_not_found {
            get {
                return ResourceManager.GetString("declaration_not_found", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to _default.
        /// </summary>
        internal static string default_class {
            get {
                return ResourceManager.GetString("default_class", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to _module.
        /// </summary>
        internal static string default_module {
            get {
                return ResourceManager.GetString("default_module", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning: This symbol was created using general Visit(Expression o). A specific overload should be implemented..
        /// </summary>
        internal static string general_expression_visit_used {
            get {
                return ResourceManager.GetString("general_expression_visit_used", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to _programRootNode.
        /// </summary>
        internal static string root_node {
            get {
                return ResourceManager.GetString("root_node", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SymbolCreationError: Symbol is not stated as declaration, but neither was a declarationOrigin given..
        /// </summary>
        internal static string symbol_creation_declaration_error {
            get {
                return ResourceManager.GetString("symbol_creation_declaration_error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TypeRhs shoudl ahve a UserDefinedType.
        /// </summary>
        internal static string typeRHS_vs_UserDefinedType {
            get {
                return ResourceManager.GetString("typeRHS_vs_UserDefinedType", resourceCulture);
            }
        }
    }
}
