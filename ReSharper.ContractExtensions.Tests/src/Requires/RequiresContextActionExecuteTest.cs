using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class RequiresContextActionExecuteTest : CSharpContextActionExecuteTestBase<RequiresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "Requires"; }
        }

        [TestCase("Execution")]
        [TestCase("ExecutionWithExistingUsing")]
        [TestCase("ExecutionWithSpecifiedOrder")]
        [TestCase("ExecutionWithSpecifiedOrder2")]
        [TestCase("ExecutionWithSpecifiedOrder3")]
        [Test]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}