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
    public class CompilationResults {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal CompilationResults() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DafnyLanguageServer.Resources.CompilationResults", typeof(CompilationResults).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Compilation failed: .
        /// </summary>
        public static string compilation_failed {
            get {
                return ResourceManager.GetString("compilation_failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Compile CLO is explicitly set to 0..
        /// </summary>
        public static string compile_CLO_is_zero {
            get {
                return ResourceManager.GetString("compile_CLO_is_zero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not locate file:.
        /// </summary>
        public static string could_not_locate_file {
            get {
                return ResourceManager.GetString("could_not_locate_file", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .dfy.
        /// </summary>
        public static string file_ending {
            get {
                return ResourceManager.GetString("file_ending", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while preprocessing your custom command line arguments..
        /// </summary>
        public static string not_supported_custom_args {
            get {
                return ResourceManager.GetString("not_supported_custom_args", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can only compile .dfy files..
        /// </summary>
        public static string only_dfy {
            get {
                return ResourceManager.GetString("only_dfy", resourceCulture);
            }
        }
    }
}
