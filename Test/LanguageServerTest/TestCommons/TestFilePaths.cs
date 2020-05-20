using System.IO;

namespace TestCommons
{

    public static class Paths
    {
        //Folders
        public static readonly string assemblyPath = Path.GetDirectoryName(typeof(Paths).Assembly.Location);
        public static readonly string rootPath = Path.GetFullPath(Path.Combine(assemblyPath, "../"));
        public static readonly string testFilesPath = Path.GetFullPath(Path.Combine(rootPath, "Test/LanguageServerTest/DafnyFiles"));

        //Executables
        public static readonly string dafnyExe = Path.GetFullPath(Path.Combine(rootPath, "Binaries/Dafny.exe"));
        public static readonly string langServExe = Path.GetFullPath(Path.Combine(rootPath, "Binaries/DafnyLanguageServer.exe"));

        /***CONFIG READER TEST***/
        /************************/
        public static readonly string cr_default = CreateTestfilePath("configReader/default.json");
        public static readonly string cr_fine = CreateTestfilePath("configReader/fine.json");
        public static readonly string cr_sameFiles = CreateTestfilePath("configReader/sameFiles.json");
        public static readonly string cr_missingFields = CreateTestfilePath("configReader/missingFields.json");
        public static readonly string cr_additionalFields = CreateTestfilePath("configReader/additionalFields.json");
        public static readonly string cr_otherEnding = CreateTestfilePath("configReader/otherEnding.json");
        public static readonly string cr_noEnding = CreateTestfilePath("configReader/noEnding.json");
        public static readonly string cr_backslashes = CreateTestfilePath("configReader/backslashes.json");
        public static readonly string cr_empty = CreateTestfilePath("configReader/empty.json");
        public static readonly string cr_nojsony = CreateTestfilePath("configReader/nojson.json");

        public static readonly string cr_wrongLogLevel = CreateTestfilePath("configReader/exceedingLogLevel.json");
        public static readonly string cr_wrongLogLevelType = CreateTestfilePath("configReader/wrongLogLevelType.json");
        public static readonly string cr_wrongStreamPathType = CreateTestfilePath("configReader/wrongStreamPathType.json");
        public static readonly string cr_wrongLogPathType = CreateTestfilePath("configReader/wrongLogPathType.json");

        /***DAFNY SOURCE FILES***/
        /************************/
        //Compile
        public static readonly string cp_fineDLL = CreateTestfilePath("compile/compiles_as_dll.dfy");
        public static readonly string cp_fineEXE = CreateTestfilePath("compile/compiles_as_exe.dfy");
        public static readonly string cp_assertion = CreateTestfilePath("compile/assertion_violation.dfy");
        public static readonly string cp_identifier = CreateTestfilePath("compile/unknown_identifier_error.dfy");
        public static readonly string cp_postcondition = CreateTestfilePath("compile/postcondition_violation.dfy");
        public static readonly string cp_semiexpected = CreateTestfilePath("compile/semi_expected_error.dfy");
        public static readonly string cp_include_main = CreateTestfilePath("compile/include_main.dfy");
        public static readonly string cp_empty = CreateTestfilePath("compile/empty.dfy");
        public static readonly string cp_otherlang_java = CreateTestfilePath("compile/otherlang_java.java");
        public static readonly string cp_otherlang_java_dfyending = CreateTestfilePath("compile/otherlang_java.dfy");
        public static readonly string cp_otherlang_py = CreateTestfilePath("compile/otherlang_python.py");
        public static readonly string cp_otherlang_py_dfyending = CreateTestfilePath("compile/otherlang_python.dfy");

        //Compile Outputs
        public static readonly string cp_out_dll = AddTestFolderPrefix("compile/compiles_as_dll.dll");
        public static readonly string cp_out_exe = AddTestFolderPrefix("compile/compiles_as_exe.exe");

        //Counter Example
        public static readonly string ce_fail1 = CreateTestfilePath("counterExample/postcondition_violation_1.dfy");
        public static readonly string ce_fail2 = CreateTestfilePath("counterExample/postcondition_violation_2.dfy");
        public static readonly string ce_ok = CreateTestfilePath("counterExample/postcondition_fullfilled.dfy");
        public static readonly string ce_float = CreateTestfilePath("counterExample/float.dfy");
        public static readonly string ce_bool = CreateTestfilePath("counterExample/bool.dfy");
        public static readonly string ce_string = CreateTestfilePath("counterExample/string.dfy");
        public static readonly string ce_set = CreateTestfilePath("counterExample/set.dfy");
        public static readonly string ce_sequence = CreateTestfilePath("counterExample/sequence.dfy");
        public static readonly string ce_2m = CreateTestfilePath("counterExample/two_methods.dfy");
        public static readonly string ce_2mc = CreateTestfilePath("counterExample/two_connected_methods.dfy");
        public static readonly string ce_lp = CreateTestfilePath("counterExample/largerprogram.dfy");
        public static readonly string ce_li = CreateTestfilePath("counterExample/loopinvariant.dfy");

        public static readonly string ce_fail1_bvd = CreateTestfilePath("counterExample/postcondition_violation_1.bvd");
        public static readonly string ce_fail2_bvd = CreateTestfilePath("counterExample/postcondition_violation_2.bvd");
        public static readonly string ce_float_bvd = CreateTestfilePath("counterExample/float.bvd");
        public static readonly string ce_bool_bvd = CreateTestfilePath("counterExample/bool.bvd");
        public static readonly string ce_string_bvd = CreateTestfilePath("counterExample/string.bvd");
        public static readonly string ce_2m_bvd = CreateTestfilePath("counterExample/two_methods.bvd");
        public static readonly string ce_2mc_bvd = CreateTestfilePath("counterExample/two_connected_methods.bvd");
        public static readonly string ce_li_bvd = CreateTestfilePath("counterExample/loopinvariant.bvd");


        //Integration
        public static readonly string int_demofile = CreateTestfilePath("integration_demofile.dfy");
        public static readonly string int_inexistant = AddTestFolderPrefix("IDONOTEXIST.dfy");

        //Goto
        public static readonly string gt_goto = CreateTestfilePath("goto/goto.dfy");
        public static readonly string gt_include_main = CreateTestfilePath("goto/include_main.dfy");
        public static readonly string gt_includee = CreateTestfilePath("goto/include_includee.dfy");

        // CodeLens
        public static readonly string cl_basic = CreateTestfilePath("codelens/basic.dfy");
        public static readonly string cl_include_main = CreateTestfilePath("codelens/include_main.dfy");
        public static readonly string cl_includee = CreateTestfilePath("codelens/include_includee.dfy");
        public static readonly string cl_empty = CreateTestfilePath("codelens/empty.dfy");

        //AutoCompletion
        public static readonly string ac_basic_var = CreateTestfilePath("autocompletion/basic_var.dfy");
        public static readonly string ac_basic_var_class = CreateTestfilePath("autocompletion/basic_var_and_class.dfy");
        public static readonly string ac_multiple_classes = CreateTestfilePath("autocompletion/basic_multiple_classes.dfy");
        public static readonly string ac_empty = CreateTestfilePath("autocompletion/empty.dfy");
        public static readonly string ac_c_empty = CreateTestfilePath("autocompletion/class_empty.dfy");
        public static readonly string ac_c_one_method = CreateTestfilePath("autocompletion/class_one_method.dfy");
        public static readonly string ac_c_multiple = CreateTestfilePath("autocompletion/class_multiple.dfy");
        public static readonly string ac_c_in_class = CreateTestfilePath("autocompletion/class_in_class.dfy");
        public static readonly string ac_c_partial = CreateTestfilePath("autocompletion/class_partial.dfy");
        public static readonly string ac_include_main = CreateTestfilePath("autocompletion/include_main.dfy");
        public static readonly string ac_includee = CreateTestfilePath("autocompletion/include_includee.dfy");

        //Verification
        public static readonly string vc_lo_assertion = CreateTestfilePath("verification/moreless_assertion.dfy");
        public static readonly string vc_lo_good = CreateTestfilePath("verification/moreless_good.dfy");
        public static readonly string vc_lo_postcondition = CreateTestfilePath("verification/moreless_postcondition.dfy");
        public static readonly string vc_lo_noensure = CreateTestfilePath("verification/moreless_noensure.dfy");
        public static readonly string vc_lo_multiplefails = CreateTestfilePath("verification/moreless_manyfails.dfy");

        public static readonly string vc_sy_curly = CreateTestfilePath("verification/syntax_curlyexpected.dfy");
        public static readonly string vc_sy_eof = CreateTestfilePath("verification/syntax_EOFexpected.dfy");
        public static readonly string vc_sy_ns = CreateTestfilePath("verification/syntax_invalidNameSegment.dfy");
        public static readonly string vc_sy_update = CreateTestfilePath("verification/syntax_invalidUpdate.dfy");
        public static readonly string vc_sy_suffix = CreateTestfilePath("verification/syntax_invalidSuffix.dfy");
        public static readonly string vc_sy_parenthesis = CreateTestfilePath("verification/syntax_parenexpected.dfy");
        public static readonly string vc_sy_bracer = CreateTestfilePath("verification/syntax_rbrace.dfy");
        public static readonly string vc_sy_semi = CreateTestfilePath("verification/syntax_semi.dfy");

        public static readonly string vc_re_args = CreateTestfilePath("verification/resolver_arguments.dfy");
        public static readonly string vc_re_type = CreateTestfilePath("verification/resolver_undeclaredType.dfy");
        public static readonly string vc_re_identifier = CreateTestfilePath("verification/resolver_identifier.dfy");

        public static readonly string vc_warning_include = CreateTestfilePath("verification/include_includer_warning.dfy");
        public static readonly string vc_warning = CreateTestfilePath("verification/warning.dfy");
        public static readonly string vc_information = CreateTestfilePath("verification/information.dfy");


        //SymbolTAble
        public static readonly string st_01 = CreateTestfilePath("symbolTable/01_basic.dfy");
        public static readonly string st_02 = CreateTestfilePath("symbolTable/02_classAccessors.dfy");
        public static readonly string st_03 = CreateTestfilePath("symbolTable/03_blockscope.dfy");
        public static readonly string st_04 = CreateTestfilePath("symbolTable/04_declarationAfterUsage.dfy");
        public static readonly string st_05 = CreateTestfilePath("symbolTable/05_globalScope.dfy");
        public static readonly string st_06 = CreateTestfilePath("symbolTable/06_GoTo.dfy");
        public static readonly string st_07 = CreateTestfilePath("symbolTable/07_if_while.dfy");
        public static readonly string st_08 = CreateTestfilePath("symbolTable/08_unary_ternary.dfy");
        public static readonly string st_09 = CreateTestfilePath("symbolTable/09_traits.dfy");
        public static readonly string st_10 = CreateTestfilePath("symbolTable/10_logical_statements.dfy");
        public static readonly string st_11 = CreateTestfilePath("symbolTable/11_modules.dfy");
        public static readonly string st_12 = CreateTestfilePath("symbolTable/12_nestedModules.dfy");
        public static readonly string st_13 = CreateTestfilePath("symbolTable/13_moduleFromDefaultScope.dfy");
        public static readonly string st_14 = CreateTestfilePath("symbolTable/14_includedModules.dfy");


        public static readonly string st_01e = CreateTestfilePath("symbolTable/01_expect.txt");
        public static readonly string st_02e = CreateTestfilePath("symbolTable/02_expect.txt");
        public static readonly string st_03e = CreateTestfilePath("symbolTable/03_expect.txt");
        public static readonly string st_04e = CreateTestfilePath("symbolTable/04_expect.txt");
        public static readonly string st_05e = CreateTestfilePath("symbolTable/05_expect.txt");
        public static readonly string st_06e = CreateTestfilePath("symbolTable/06_expect.txt");
        public static readonly string st_07e = CreateTestfilePath("symbolTable/07_expect.txt");
        public static readonly string st_08e = CreateTestfilePath("symbolTable/08_expect.txt");
        public static readonly string st_09e = CreateTestfilePath("symbolTable/09_expect.txt");
        public static readonly string st_10e = CreateTestfilePath("symbolTable/10_expect.txt");
        public static readonly string st_11e = CreateTestfilePath("symbolTable/11_expect.txt");
        public static readonly string st_12e = CreateTestfilePath("symbolTable/12_expect.txt");
        public static readonly string st_13e = CreateTestfilePath("symbolTable/13_expect.txt");
        public static readonly string st_14e = CreateTestfilePath("symbolTable/14_expect.txt");


        //Rename
        public static readonly string rn_scopes = CreateTestfilePath("rename/renameWithScopes.dfy");
        public static readonly string rn_include_main = CreateTestfilePath("rename/include_main.dfy");
        public static readonly string rn_includee = CreateTestfilePath("rename/include_includee.dfy");




        private static string CreateTestfilePath(string s)
        {
            var path = AddTestFolderPrefix(s);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            return path;
        }

        private static string AddTestFolderPrefix(string s) => Path.GetFullPath(Path.Combine(testFilesPath, s));
    }
}