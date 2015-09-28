using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Ensures;

namespace ReSharper.ContractExtensions.Tests.Postconditions
{
    [TestFixture]
    public class ComboEnsuresContextActionExecuteTest : CSharpContextActionExecuteTestBase<ComboEnsuresContextAction>
    {
        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/ComboEnsures"; }
        }

        protected override string ExtraPath
        {
            get { return "ComboEnsures"; }
        }

        [TestCase("ExecutionForAbstractMethod")]
        [TestCase("ExecutionForInterface")]
        [TestCase("ExecutionForPartiallyDefinedContract")]
        [TestCase("ExecutionForAbstractProperty")]
        [TestCase("ExecutionForPropertyInTheInterface")]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }

    }
}