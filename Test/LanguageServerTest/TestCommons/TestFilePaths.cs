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

        /***DAFNY SOURCE FILES***/
        /************************/

        //Compile
        public static readonly string cp_fineDLL = CreateTestfilePath("compile/compiles_as_dll.dfy");
        public static readonly string cp_fineEXE = CreateTestfilePath("compile/compiles_as_exe.dfy");
        public static readonly string cp_assertion = CreateTestfilePath("compile/assertion_violation.dfy");
        public static readonly string cp_identifier = CreateTestfilePath("compile/unknown_identifier_error.dfy");
        public static readonly string cp_postcondition = CreateTestfilePath("compile/postcondition_violation.dfy");
        public static readonly string cp_semiexpected = CreateTestfilePath("compile/semi_expected_error.dfy");
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


        //Integration
        public static readonly string int_demofile = CreateTestfilePath("integration_demofile.dfy");
        public static readonly string int_inexistant = AddTestFolderPrefix("IDONOTEXIST.dfy");

        //Goto
        public static readonly string gt_goto = CreateTestfilePath("goto/goto.dfy");

        //Include
        public static readonly string ic_basic = CreateTestfilePath("include/basic.dfy");
        public static readonly string ic_includee = CreateTestfilePath("include/includee.dfy");

        //AutoCompletion
        public static readonly string ac_ac = CreateTestfilePath("autocompletion/autocompletion.dfy");

        //Verification
        public static readonly string vc_lo_assertion = CreateTestfilePath("verification/moreless_assertion.dfy");
        public static readonly string vc_lo_good = CreateTestfilePath("verification/moreless_good.dfy");
        public static readonly string vc_lo_postcondition = CreateTestfilePath("verification/moreless_postcondition.dfy");
        public static readonly string vc_lo_noensure = CreateTestfilePath("verification/moreless_noensure.dfy");
        public static readonly string vc_lo_multiplefails = CreateTestfilePath("verification/moreless_manyfails.dfy");

        public static readonly string vc_sy_curly = CreateTestfilePath("verification/syntax_curlyexpected.dfy");
        public static readonly string vc_sy_eof = CreateTestfilePath("verification/syntax_EOFexpected.dfy");
        public static readonly string vc_sy_ns = CreateTestfilePath("verification/syntax_invalidNameSegment.dfy");
        public static readonly string vc_sy_ns2 = CreateTestfilePath("verification/syntax_invalidNameSegment2.dfy");
        public static readonly string vc_sy_suffix = CreateTestfilePath("verification/syntax_invalidSuffix.dfy");
        public static readonly string vc_sy_parenthesis = CreateTestfilePath("verification/syntax_parenexpected.dfy");
        public static readonly string vc_sy_bracer = CreateTestfilePath("verification/syntax_rbrace.dfy");
        public static readonly string vc_sy_semi = CreateTestfilePath("verification/syntax_semi.dfy");

        public static readonly string vc_se_args = CreateTestfilePath("verification/semantic_arguments.dfy");
        public static readonly string vc_se_type = CreateTestfilePath("verification/semantic_undeclaredType.dfy");
        public static readonly string vc_se_identifier = CreateTestfilePath("verification/semantic_identifier.dfy");


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