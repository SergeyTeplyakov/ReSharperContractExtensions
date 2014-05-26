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

        [TestCase("ExecutionForAbstractMethod")]
        [TestCase("ExecutionForInterface")]

        [TestCase("ExecutionWithExistingUsing")]

        [TestCase("ExecutionWithSpecifiedOrder")]
        [TestCase("ExecutionWithSpecifiedOrder2")]
        [TestCase("ExecutionWithSpecifiedOrder3")]
        [TestCase("ExecutionWithSpecifiedOrder4")]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}