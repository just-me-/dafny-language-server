﻿using System.IO;

namespace PathConstants
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

        //Compile: Dafny Sourcefiles
        public static readonly string cp_fineDLL = "compile_as_dll.dfy";
        public static readonly string cp_fineEXE = "compile_as_exe.dfy";
        public static readonly string cp_assertion = "compile_assertion_violation.dfy";
        public static readonly string cp_identifier = "compile_unknown_identifier_error.dfy";
        public static readonly string cp_postcondition = "compile_postcondition_violation.dfy";

        //Compile Outputs
        public static readonly string cp_out_dll = "compiles_as_dll.dll";
        public static readonly string cp_out_exe = "compiles_as_exe.exe";

        //Counter Example Dafny Sourcefiles
        public static readonly string ce_fail1 = "ce_postcondition_violation_1.dfy";
        public static readonly string ce_fail2 = "ce_postcondition_violation_2.dfy";
        public static readonly string ce_ok = "ce_postcondition_fullfilled.dfy";
        public static readonly string ce_2meth = "ce_two_methods.dfy";
    }
}