using System;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using DafnyLanguageServer;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace ProgramTest
{
    public class ConfigReaderTest
    {

        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(ConfigReaderTest).Assembly.Location);
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
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsFalse(cr.Error);
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        [Test]
        public void Fine()
        {
            string configFile = Files.cr_fine;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("../Binaries/b.txt");
            string expectedStreamFile = CombineWithDefaultLogFolder("../Binaries/a.txt");
            LogLevel expectedLogLevel = LogLevel.Trace;


            Assert.IsFalse(cr.Error);
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        [Test]
        public void SameFiles()
        {
            string configFile = Files.cr_sameFiles;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsTrue(cr.Error);
            Assert.IsTrue(cr.ErrorMsg.Contains("same files"));
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        [Test]
        public void MissingFields()
        { 
                string configFile = Files.cr_missingFields;
                var cr = new ConfigReader(new string[] { }, configFile);

                string expectedLogFile = defaultLog;
                string expectedStreamFile = defaultStream;
                LogLevel expectedLogLevel = LogLevel.Error;

                Console.WriteLine(cr.ErrorMsg);

            Assert.IsFalse(cr.Error);
                Assert.AreEqual(expectedLogFile, cr.LogFile);
                Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
                Assert.AreEqual(expectedLogLevel, cr.Loglevel);
            
        }

        public void AdditionalFields()
        {
            string configFile = Files.cr_additionalFields;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsFalse(cr.Error);
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);

        }

        public void OtherFileExtension()
        {
            string configFile = Files.cr_otherEnding;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("StreamRedirection.log");
            string expectedStreamFile = CombineWithDefaultLogFolder("Log.log");
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsFalse(cr.Error);
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);

        }

        public void NoFileExtension()
        {
            string configFile = Files.cr_noEnding;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("StreamRedirection");
            string expectedStreamFile = CombineWithDefaultLogFolder("Log");
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsFalse(cr.Error);
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);

        }

        [Test]
        public void BackslashesEscaped()
        {
            string configFile = Files.cr_backslashesEscaped;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsFalse(cr.Error);
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        [Test]
        public void Backslashes()
        {
            string configFile = Files.cr_backslashes;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsTrue(cr.Error);
            Assert.IsTrue(cr.ErrorMsg.Contains("escape sequence"));
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        [Test]
        public void Empty()
        {
            string configFile = Files.cr_empty;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsTrue(cr.Error);
            Assert.IsTrue(cr.ErrorMsg.Contains("parsing"));
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        [Test]
        public void NoJson()
        {
            string configFile = Files.cr_nojsony;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsTrue(cr.Error);
            Assert.IsTrue(cr.ErrorMsg.Contains("parsing"));
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        [Test]
        public void ExceedingLogLevel()
        {
            string configFile = Files.cr_wrongLogLevel;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsTrue(cr.Error);
            Assert.IsTrue(cr.ErrorMsg.Contains("out of bounds"));
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        [Test]
        public void WrongLogLevelType()
        {
            string configFile = Files.cr_wrongLogLevelType;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(cr.ErrorMsg);

            Assert.IsTrue(cr.Error);
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        [Test]
        public void WrongStreamPathType()
        {
            string configFile = Files.cr_wrongStreamPathType;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = defaultLog;
            string expectedStreamFile = CombineWithDefaultLogFolder("../Binaries/1"); //int as path will just use this as file name
            LogLevel expectedLogLevel = LogLevel.Error;

            Console.WriteLine(cr.ErrorMsg);

            Assert.IsFalse(cr.Error);
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }


        [Test]
        public void WrongLogPathType()
        {
            string configFile = Files.cr_wrongLogPathType;
            var cr = new ConfigReader(new string[] { }, configFile);

            string expectedLogFile = CombineWithDefaultLogFolder("../Binaries/2");
            string expectedStreamFile = defaultStream;
            LogLevel expectedLogLevel = LogLevel.Error;


            Assert.IsFalse(cr.Error);
            Assert.AreEqual(expectedLogFile, cr.LogFile);
            Assert.AreEqual(expectedStreamFile, cr.RedirectedStreamFile);
            Assert.AreEqual(expectedLogLevel, cr.Loglevel);
        }

        #endregion

    }
}