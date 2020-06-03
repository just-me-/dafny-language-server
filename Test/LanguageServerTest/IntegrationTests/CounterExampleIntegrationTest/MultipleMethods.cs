using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace CounterExampleIntegrationTest
{
    [TestFixture]
    public class MultipleMethods : CounterExampleBase
    {
        //Tests still occasionally fail... :(
        //Has something to do with fileaccess on the bvd file
        //Not sure if boogie locks itself from the file.

        //[Test]
        public void TwoMethods()
        {
            GetCounterExamples(Files.ce_2m);
            List<string> expecation = new List<string>()
            {
                "L3 C19: in1 = 2446; ",
                "L9 C19: in2 = - 160; "
            };
            VerifyResults(expecation);
        }

        //[Test]
        public void TwoConnectedMethods()
        {
            GetCounterExamples(Files.ce_2mc);
            List<string> expecation = new List<string>()
            {
                "L3 C19: in1 = 2446; ",
                "L9 C19: in2 = 891; "
            };
            VerifyResults(expecation);
        }


    }
}