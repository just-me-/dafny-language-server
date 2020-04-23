using System;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using DafnyLanguageServer;
using DafnyLanguageServer.ProgramServices;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace ProgramTest
{
    public class ConfigInitializerTest
    {

        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(ConfigInitializerTest).Assembly.Location);
        private static readonly string defaultLogPath = CombinePath(assemblyPath, "../Logs");
        private static string CombinePath(string path, string file) => Path.GetFullPath(Path.Combine(path, file));
        private static string CombineWithDefaultLogFolder(string file) => CombinePath(defaultLogPath, file);

        private static string defaultLog = CombineWithDefaultLogFolder("Log.txt");
        private static string defaultStream = CombineWithDefaultLogFolder("StreamRedirection.txt");


        #region WithCfgFiles
        [Test]
        public void Defaults()
        {
            string configFile = Files.cr_default;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsFalse(ci.Config.Error);
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void Fine()
        {
            string configFile = Files.cr_fine;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("../Binaries/b.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("../Binaries/a.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;

            Console.WriteLine(ci.Config);


            Assert.IsFalse(ci.Config.Error);
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void SameFiles()
        {
            string configFile = Files.cr_sameFiles;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);


            Assert.IsTrue(ci.Config.Error);
            Assert.IsTrue(ci.Config.ErrorMsg.Contains("same files"));
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void MissingFields()
        {
            string configFile = Files.cr_missingFields;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsFalse(ci.Config.Error);
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);

        }

        public void AdditionalFields()
        {
            string configFile = Files.cr_additionalFields;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsFalse(ci.Config.Error);
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);

        }

        public void OtherFileExtension()
        {
            string configFile = Files.cr_otherEnding;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("StreamRedirection.log");
            string expectedStreamFile = CombineWithDefaultLogFolder("Log.log");
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsFalse(ci.Config.Error);
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);

        }

        public void NoFileExtension()
        {
            string configFile = Files.cr_noEnding;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("StreamRedirection");
            string expectedStreamFile = CombineWithDefaultLogFolder("Log");
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsFalse(ci.Config.Error);
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);

        }


        [Test]
        public void Backslashes()
        {
            string configFile = Files.cr_backslashes;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error);
            Assert.IsTrue(ci.Config.ErrorMsg.Contains("escape sequence"));
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void Empty()
        {
            string configFile = Files.cr_empty;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error);
            Assert.IsTrue(ci.Config.ErrorMsg.Contains("parsing"));
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void NoJson()
        {
            string configFile = Files.cr_nojsony;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error);
            Assert.IsTrue(ci.Config.ErrorMsg.Contains("parsing"));
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void ExceedingLogLevel()
        {
            string configFile = Files.cr_wrongLogLevel;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error);
            Assert.IsTrue(ci.Config.ErrorMsg.Contains("exceeds"));
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void WrongLogLevelType()
        {
            string configFile = Files.cr_wrongLogLevelType;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error);
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void WrongStreamPathType()
        {
            string configFile = Files.cr_wrongStreamPathType;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = CombineWithDefaultLogFolder("../Binaries/1"); //int as path will just use this as file name
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsFalse(ci.Config.Error);
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }


        [Test]
        public void WrongLogPathType()
        {
            string configFile = Files.cr_wrongLogPathType;
            var ci = new ConfigInitializer(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("../Binaries/2");
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsFalse(ci.Config.Error);
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
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

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error, "Error Mismatch");
            Assert.IsTrue(ci.Config.ErrorMsg.Contains("file not found"));
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        #endregion

        #region Arguments


        [Test]
        public void SunnyCase()
        {
            string configFile = CombinePath("/", "abasdfasfdsa.json");

            var ci = new ConfigInitializer(new string[] { "/log:../Logs/a.txt", "/stream:../Logs/b.txt", "/loglevel:0" }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("a.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("b.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }


        [Test]
        public void MissingCfGFile()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/log:../Logs/a.txt", "/stream:../Logs/b.txt", "/loglevel:0" }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("a.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("b.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;

            Console.WriteLine(ci.Config);

            Assert.IsFalse(ci.Config.Error, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void IllegalArgument()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/abc:ab" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error, "Error Mismatch");
            Assert.IsTrue(ci.Config.ErrorMsg.Contains("Unknown switch"));

            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void NoValue()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/log" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error, "Error Mismatch");
            Assert.IsTrue(ci.Config.ErrorMsg.Contains("parsing"));

            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void NoValue2()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/log:" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error, "Error Mismatch");
            Assert.IsTrue(ci.Config.ErrorMsg.Contains("No Argument provided"));


            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void IllegalLogLevel()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/loglevel:Yes Please Log" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        [Test]
        public void ExceedingLogLevelInArgs()
        {
            string configFile = Files.cr_default;

            var ci = new ConfigInitializer(new string[] { "/loglevel:55" }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(ci.Config);

            Assert.IsTrue(ci.Config.Error, "Error Mismatch");
            Assert.AreEqual(expectedLogFile, ci.Config.LogFile);
            Assert.AreEqual(expectedStreamFile, ci.Config.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, ci.Config.Loglevel);
        }

        #endregion
    }
}