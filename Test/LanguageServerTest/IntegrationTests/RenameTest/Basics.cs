using NUnit.Framework;
using Files = TestCommons.Paths;

namespace RenameTest
{
    public class Tests : RenameBase
    {

        protected string f = Files.rn_scopes;

        [Test]
        public void Test1()
        {
            Run(f);
            Verify();
        }
    }
}