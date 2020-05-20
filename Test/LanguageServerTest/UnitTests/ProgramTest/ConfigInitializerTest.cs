using System;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using DafnyLanguageServer;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Resources;
using DafnyLanguageServer.Tools;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace ProgramTest
{
    public class ConfigInitializerTest
    {

        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(ConfigInitializerTest).Assembly.Location);
        private static readonly string defaultLogPath = FileAndFolderLocator.defaultLogFile;
        private static string CombinePath(string path, string file) => Path.GetFullPath(Path.Combine(path, file));
        private static string CombineWithDefaultLogFolder(string file) => CombinePath(defaultLogPath, file);

        private static string defaultLog = FileAndFolderLocator.defaultLogFile;
        private static string defaultStream = FileAndFolderLocator.defaultStreamFile;


        #region WithCfgFiles
        [Test]
        public void Defaults()
        {
            string configFile = Files.cr_default;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.InitializationErrors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void Fine()
        {
            string configFile = Files.cr_fine;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("./Binaries/b.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("./Binaries/a.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;

            Console.WriteLine(LanguageServerConfig.ToString());


            Assert.IsFalse(ci.InitializationErrors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void SameFiles()
        {
            string configFile = Files.cr_sameFiles;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());


            Assert.IsTrue(ci.InitializationErrors.HasErrors);
            Assert.IsTrue(ci.InitializationErrors.ErrorMessages.Contains("same files"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void MissingFields()
        {
            string configFile = Files.cr_missingFields;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.InitializationErrors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);

        }

        public void AdditionalFields()
        {
            string configFile = Files.cr_additionalFields;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.InitializationErrors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);

        }

        public void OtherFileExtension()
        {
            string configFile = Files.cr_otherEnding;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("StreamRedirection.log");
            string expectedStreamFile = CombineWithDefaultLogFolder("Log.log");
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.InitializationErrors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);

        }

        public void NoFileExtension()
        {
            string configFile = Files.cr_noEnding;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("StreamRedirection");
            string expectedStreamFile = CombineWithDefaultLogFolder("Log");
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.InitializationErrors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);

        }


        [Test]
        public void Backslashes()
        {
            string configFile = Files.cr_backslashes;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors);
            Assert.IsTrue(ci.InitializationErrors.ErrorMessages.Contains("escape sequence"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void Empty()
        {
            string configFile = Files.cr_empty;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors);
            Assert.IsTrue(ci.InitializationErrors.ErrorMessages.Contains("parsing"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void NoJson()
        {
            string configFile = Files.cr_nojsony;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors);
            Assert.IsTrue(ci.InitializationErrors.ErrorMessages.Contains("parsing"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void ExceedingLogLevel()
        {
            string configFile = Files.cr_wrongLogLevel;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors);
            Assert.IsTrue(ci.InitializationErrors.ErrorMessages.Contains("exceeds"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void WrongLogLevelType()
        {
            string configFile = Files.cr_wrongLogLevelType;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void WrongStreamPathType()
        {
            string configFile = Files.cr_wrongStreamPathType;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = CombineWithDefaultLogFolder("./Binaries/1"); //int as path will just use this as file name
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.InitializationErrors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }


        [Test]
        public void WrongLogPathType()
        {
            string configFile = Files.cr_wrongLogPathType;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("./Binaries/2");
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.InitializationErrors.HasErrors);
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        #endregion

        #region defaults
        [Test]
        public void NoCfgFile()
        {
            string configFile = CombinePath("/", "abasdfasfdsa.json");
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors, "Error Mismatch");
            Assert.IsTrue(ci.InitializationErrors.ErrorMessages.Contains("file not found"));
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        #endregion

        #region Arguments


        [Test]
        public void SunnyCase()
        {
            string configFile = CombinePath("/", "abasdfasfdsa.json");

            var ci = new ConfigInitializer(new string[] { "/log:./Logs/a.txt", "/stream:./Logs/b.txt", "/loglevel:0" }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("a.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("b.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }


        [Test]
        public void MissingCfGFile()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/log:./Logs/a.txt", "/stream:./Logs/b.txt", "/loglevel:0" }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("a.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("b.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsFalse(ci.InitializationErrors.HasErrors, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void IllegalArgument()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/abc:ab" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors, "Error Mismatch");
            Assert.IsTrue(ci.InitializationErrors.ErrorMessages.Contains("Unknown switch"));

            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void NoValue()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/log" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors, "Error Mismatch");
            Assert.IsTrue(ci.InitializationErrors.ErrorMessages.Contains("parsing"));

            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void NoValue2()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/log:" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors, "Error Mismatch");
            Assert.IsTrue(ci.InitializationErrors.ErrorMessages.Contains("No Argument provided"));


            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void IllegalLogLevel()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/loglevel:Yes Please Log" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        [Test]
        public void ExceedingLogLevelInArgs()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/loglevel:55" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(LanguageServerConfig.ToString());

            Assert.IsTrue(ci.InitializationErrors.HasErrors, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, LanguageServerConfig.LogFile);
            Assert.AreEqual(expectedStreamFile, LanguageServerConfig.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, LanguageServerConfig.LogLevel);
        }

        #endregion
    }
}