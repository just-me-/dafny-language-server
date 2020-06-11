using NUnit.Framework;
using System.Collections.Generic;
using Files = TestCommons.Paths;

namespace AutoCompletionIntegrationTest
{
    [TestFixture]
    public class ParameterTest : AutoCompletionBase
    {
        [Test]
        public void CheckClass()
        {
            int line = 16;
            int col = 9;
            var exp = new List<string>()
            {
                "noParam()",
                "oneParam()",
                "twoParam()",
            };
            GetCompletions(Files.ac_params, line, col);
            VerifyCompletions(exp);
            var expParams = new List<string>()
            {
                "",
                "a: int",
                "a: int, b: int",
            };
            VerifyParameters(expParams);
        }

        [Test]
        public void CheckMethod()
        {
            int line = 17;
            int col = 4;
            var exp = new List<string>()
            {
                "a",
                "C",
                "Main",
                "MyMethod",
            };
            GetCompletions(Files.ac_params, line, col);
            VerifyCompletions(exp);
            var expParams = new List<string>()
            {
                "",
                "",
                "",
                "a: int, b: int, c: int",
            };
            VerifyParameters(expParams);
        }
    }
}