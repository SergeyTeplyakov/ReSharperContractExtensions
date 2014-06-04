using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class RequiresContextActionExecuteTest : RequiresContextActionExecuteTestBase<ArgumentRequiresContextAction>
    {

        protected override string ExtraPath
        {
            get { return "Requires"; }
        }

        [TestCase("Execution")]

        [TestCase("ExecutionForAbstractMethod")]

        [TestCase("ExecutionOnPropertySetter")]
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