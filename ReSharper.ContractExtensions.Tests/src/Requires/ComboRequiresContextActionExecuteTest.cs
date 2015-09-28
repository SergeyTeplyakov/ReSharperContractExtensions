using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class ComboRequiresContextActionExecuteTest : RequiresContextActionExecuteTestBase<ComboRequiresContextAction>
    {
        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/ComboRequires"; }
        }
        protected override string ExtraPath
        {
            get { return "ComboRequires"; }
        }

        [TestCase("ExecutionForAbstractMethod")]
        [TestCase("ExecutionForInterface")]
        [TestCase("ExecutionForAbstractProperty")]
        [TestCase("ExecutionForPartiallyDefinedContract")]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}