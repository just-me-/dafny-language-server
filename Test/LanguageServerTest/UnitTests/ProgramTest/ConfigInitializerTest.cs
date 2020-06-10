using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Tools.ConfigInitialization;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using System;
using System.IO;
using Files = TestCommons.Paths;

namespace ProgramTest
{
    public class ConfigInitializerTest
    {
        private static readonly string defaultLogFolder = FileAndFolderLocations.logFolder;

        private static string CombinePath(string path, string file) => Path.GetFullPath(Path.Combine(path, file));

        private static string CombineWithDefaultLogFolder(string file) => CombinePath(defaultLogFolder, file);

        private static string defaultLog = FileAndFolderLocations.defaultLogFile;
        private static string defaultStream = FileAndFolderLocations.defaultStreamFile;

        #region WithCfgFiles

        [Test]
        public void Defaults()
        {
            string configFile = Files.cr_default;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
        }

        [Test]
        public void GoodCustomValues()
        {
            string configFile = Files.cr_fine;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = CombineWithDefaultLogFolder("./b.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("./a.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Full;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
        }

        [Test]
        public void SameFiles()
        {
            string configFile = Files.cr_sameFiles;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors);
            Assert.IsTrue(ci.Errors.ErrorMessages.Contains("same files"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void MissingFields()
        {
            string configFile = Files.cr_missingFields;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors, "has error mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        public void AdditionalFields()
        {
            string configFile = Files.cr_additionalFields;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        public void OtherFileExtension()
        {
            string configFile = Files.cr_otherEnding;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = CombineWithDefaultLogFolder("StreamRedirection.log");
            string expectedStreamFile = CombineWithDefaultLogFolder("Log.log");
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        public void NoFileExtension()
        {
            string configFile = Files.cr_noEnding;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = CombineWithDefaultLogFolder("StreamRedirection");
            string expectedStreamFile = CombineWithDefaultLogFolder("Log");
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void Backslashes()
        {
            string configFile = Files.cr_backslashes;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors);
            Assert.IsTrue(ci.Errors.ErrorMessages.Contains("escape sequence"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void Empty()
        {
            string configFile = Files.cr_empty;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors, "Error expectation mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void NoJson()
        {
            string configFile = Files.cr_nojsony;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors);
            Assert.IsTrue(ci.Errors.ErrorMessages.Contains("parsing"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void ExceedingLogLevel()
        {
            string configFile = Files.cr_wrongLogLevel;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors, "error expectation mismatch");
            Assert.IsTrue(ci.Errors.ErrorMessages.Contains("Loglevel must be between"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void WrongLogLevelType()
        {
            string configFile = Files.cr_wrongLogLevelType;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void WrongStreamPathType()
        {
            string configFile = Files.cr_wrongStreamPathType;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = CombineWithDefaultLogFolder("../1"); //int as path will just use this as file name
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void WrongLogPathType()
        {
            string configFile = Files.cr_wrongLogPathType;
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = CombineWithDefaultLogFolder("../2");
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        #endregion WithCfgFiles

        #region defaults

        [Test]
        public void NoCfgFile()
        {
            string configFile = CombinePath("/", "abasdfasfdsa.json");
            var ci = new ConfigInitializer(new string[] { }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors, "Error Mismatch");
            Assert.IsTrue(ci.Errors.ErrorMessages.Contains("not be located"), "Errormsg not contained");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        #endregion defaults

        #region Arguments

        [Test]
        public void SunnyCase()
        {
            string configFile = CombinePath("/", "abasdfasfdsa.json");

            var ci = new ConfigInitializer(new string[] { "/log:./Logs/a.txt", "/stream:./Logs/b.txt", "/loglevel:0" }, configFile);
            ci.SetUp();

            string expectedLogFile = CombineWithDefaultLogFolder("a.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("b.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void MissingCfGFile()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/log:./Logs/a.txt", "/stream:./Logs/b.txt", "/loglevel:0" }, configFile);
            ci.SetUp();

            string expectedLogFile = CombineWithDefaultLogFolder("a.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("b.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.Errors.HasErrors, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void IllegalArgument()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/abc:ab" }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors, "Error Mismatch");
            Assert.IsTrue(ci.Errors.ErrorMessages.Contains("Unknown switch"));

            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void NoValue()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/log" }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors, "Error Mismatch");
            Assert.IsTrue(ci.Errors.ErrorMessages.Contains("parsing"));

            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void NoValue2()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/log:" }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors, "Error Mismatch");
            Assert.IsTrue(ci.Errors.ErrorMessages.Contains("No Argument provided"));

            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void IllegalLogLevel()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/loglevel:Yes Please Log" }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void ExceedingLogLevelInArgs()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/loglevel:55" }, configFile);
            ci.SetUp();

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;
            TextDocumentSyncKind expectedSyncKind = TextDocumentSyncKind.Incremental;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.Errors.HasErrors, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedSyncKind, LanguageServerConfig.SyncKind);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        #endregion Arguments
    }
}