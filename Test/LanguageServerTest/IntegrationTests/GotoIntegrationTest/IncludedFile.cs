using NUnit.Framework;
using Files = TestCommons.Paths;

namespace GotoIntegrationTest

{
    [TestFixture]
    public class IncludedFile : GoToBase
    {
        [Test]
        public void ExternalClassDefinition()
        {
            Client.TextDocument.DidOpen(Files.gt_include_main, "dfy");
            goneTo = Client.TextDocument.Definition(Files.gt_include_main, 3 - 1, 18 - 1).Result;

            VerifyResult(Files.gt_includee, 1, 7);
        }

        [Test]
        public void MethodOfExternalClass()
        {
            Client.TextDocument.DidOpen(Files.gt_include_main, "dfy");
            goneTo = Client.TextDocument.Definition(Files.gt_include_main, 4 - 1, 17 - 1).Result;

            VerifyResult(Files.gt_includee, 3, 21);
        }

        [Test]
        public void FieldOfExternalClass()
        {
            Client.TextDocument.DidOpen(Files.gt_include_main, "dfy");
            goneTo = Client.TextDocument.Definition(Files.gt_include_main, 5 - 1, 12 - 1).Result;

            VerifyResult(Files.gt_includee, 6, 9);
        }
    }
}