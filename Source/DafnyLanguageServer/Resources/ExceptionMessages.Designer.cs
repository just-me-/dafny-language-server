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
    internal class ExceptionMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ExceptionMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DafnyLanguageServer.Resources.ExceptionMessages", typeof(ExceptionMessages).Assembly);
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
        ///   Looks up a localized string similar to When symbol is a declaration, it cannot be a usage of itself..
        /// </summary>
        internal static string cannot_use_itself {
            get {
                return ResourceManager.GetString("cannot_use_itself", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not add usage at unknown symbol..
        /// </summary>
        internal static string cannot_use_unknown_symbol {
            get {
                return ResourceManager.GetString("cannot_use_unknown_symbol", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This auto completion desire is not yet supported..
        /// </summary>
        internal static string completion_not_yet_supported {
            get {
                return ResourceManager.GetString("completion_not_yet_supported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while parsing json config..
        /// </summary>
        internal static string config_could_not_be_parsed {
            get {
                return ResourceManager.GetString("config_could_not_be_parsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file Config/LanguageServerConfig.json file could not be located..
        /// </summary>
        internal static string config_file_not_existing {
            get {
                return ResourceManager.GetString("config_file_not_existing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Config file not found at:.
        /// </summary>
        internal static string config_file_not_found {
            get {
                return ResourceManager.GetString("config_file_not_found", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while collecting (counter example) models..
        /// </summary>
        internal static string could_not_collect_models {
            get {
                return ResourceManager.GetString("could_not_collect_models", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while executing compilation..
        /// </summary>
        internal static string could_not_execute_compilation {
            get {
                return ResourceManager.GetString("could_not_execute_compilation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specific model does not contain a [ :initial state ]..
        /// </summary>
        internal static string counter_example_no_init_state {
            get {
                return ResourceManager.GetString("counter_example_no_init_state", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Internal Error constructing DTU: PhysicalFile must not be null..
        /// </summary>
        internal static string DTU_no_physical_file_given {
            get {
                return ResourceManager.GetString("DTU_no_physical_file_given", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A DafnyTranslationUnit can only be used once. Create a new one!.
        /// </summary>
        internal static string DTU_only_use_once {
            get {
                return ResourceManager.GetString("DTU_only_use_once", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cursor position is exceeding line width..
        /// </summary>
        internal static string file_cursor_exceed_line_width {
            get {
                return ResourceManager.GetString("file_cursor_exceed_line_width", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A Filerepository must habe a Physical File but it wasn&apos;t the case..
        /// </summary>
        internal static string file_repo_must_have_physical {
            get {
                return ResourceManager.GetString("file_repo_must_have_physical", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Global Class Scope was not registered..
        /// </summary>
        internal static string global_class_not_registered {
            get {
                return ResourceManager.GetString("global_class_not_registered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Arguments are illegal, e.g. start position is not before endposition..
        /// </summary>
        internal static string illegal_wrapping_args {
            get {
                return ResourceManager.GetString("illegal_wrapping_args", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Filter Operation - no results were found but at least one was expected..
        /// </summary>
        internal static string invalid_filter_operation {
            get {
                return ResourceManager.GetString("invalid_filter_operation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Module given to deep visitor as entry point..
        /// </summary>
        internal static string invalid_module_handed_to_deep_visitor {
            get {
                return ResourceManager.GetString("invalid_module_handed_to_deep_visitor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Loglevel must be between 0 and 6..
        /// </summary>
        internal static string level_out_of_bounds {
            get {
                return ResourceManager.GetString("level_out_of_bounds", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LogLevel exceeds limits. Must be between 0 and 6. Setting to default LogLevel 4 = Error..
        /// </summary>
        internal static string loglevel_illegal {
            get {
                return ResourceManager.GetString("loglevel_illegal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When symbol is not a declaration, its declarationOrigin must be given..
        /// </summary>
        internal static string missing_delcaration_origin {
            get {
                return ResourceManager.GetString("missing_delcaration_origin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Line index must not be negative..
        /// </summary>
        internal static string negativ_line {
            get {
                return ResourceManager.GetString("negativ_line", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Argument provided for switch .
        /// </summary>
        internal static string no_arg_for_switch {
            get {
                return ResourceManager.GetString("no_arg_for_switch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No class origin was found for symbol: .
        /// </summary>
        internal static string no_class_origin_found {
            get {
                return ResourceManager.GetString("no_class_origin_found", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Symbol before dot has not been found..
        /// </summary>
        internal static string no_symbol_before_fot_found {
            get {
                return ResourceManager.GetString("no_symbol_before_fot_found", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There are not enough lines in the given source..
        /// </summary>
        internal static string not_enough_lines {
            get {
                return ResourceManager.GetString("not_enough_lines", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error parsing launch arguments. Please refer to the readme.md.
        /// </summary>
        internal static string not_supported_launch_args {
            get {
                return ResourceManager.GetString("not_supported_launch_args", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to StreamRedirection and Log must not be the same files..
        /// </summary>
        internal static string stream_and_log_are_same {
            get {
                return ResourceManager.GetString("stream_and_log_are_same", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A Symbol Entrypoint must be set for navigator methods. It must not be null..
        /// </summary>
        internal static string symbol_entrypoint_must_be_set {
            get {
                return ResourceManager.GetString("symbol_entrypoint_must_be_set", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid class path... expected Module.Class pattern..
        /// </summary>
        internal static string tmp_invalid_class_path {
            get {
                return ResourceManager.GetString("tmp_invalid_class_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Every Symbol must have a token..
        /// </summary>
        internal static string token_requires {
            get {
                return ResourceManager.GetString("token_requires", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Expected string or TextDocumentChangeEvent-Container at text-document-change event request..
        /// </summary>
        internal static string unexpected_file_type {
            get {
                return ResourceManager.GetString("unexpected_file_type", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown switch: &apos;{0}&apos;. Please refer to readme.md.
        /// </summary>
        internal static string unknown_switch {
            get {
                return ResourceManager.GetString("unknown_switch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This visitor must only visit declarations..
        /// </summary>
        internal static string visit_only_declarations {
            get {
                return ResourceManager.GetString("visit_only_declarations", resourceCulture);
            }
        }
    }
}
