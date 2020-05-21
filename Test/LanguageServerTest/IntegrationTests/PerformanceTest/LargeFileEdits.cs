using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;
using Files = TestCommons.Paths;

namespace PerformanceTest
{
    public class Tests : IntegrationTestBase
    {

        public Tests() : base("Performance")
        {
        }

        [Test]
        public void Test()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Client.TextDocument.DidOpen(Files.pf_large, "dfy");
            
            var contentChanges = new TextDocumentContentChangeEvent()
            {
                Range = new Range
                {
                    Start = new Position
                    {
                        Line = 6600,
                        Character = 100
                    },
                    End = new Position
                    {
                        Line = 6600,
                        Character = 100
                    }
                },
                RangeLength = 0,
                Text = " "
            };
            for (int i = 0; i < 10; i++)
            {
                Client.SendNotification("textDocument/didChange", contentChanges);
            }

            var foo = Client.TextDocument.Hover(Files.pf_large, 6600, 100);
            //Client.IsReady.Wait();

            stopwatch.Stop();
            var elapsed_time = stopwatch.ElapsedMilliseconds;
            //Assert.Less(elapsed_time, 10e3, "Runtime takes too long!" + elapsed_time);
        }
    }
}