using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;

namespace ReSharper.ContractExtensions.Tests.Invariants
{
    [TestFixture]
    public class InvariantContextActionExecuteTest : CSharpContextActionExecuteTestBase<InvariantContextAction>
    {
        protected override string ExtraPath
        {
            get { return "Invariants"; }
        }

        [TestCase("Execution")]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }

        [TestCase("ExecutionAddAnotherInvariant")]
        [TestCase("ExecutionWhenInvariantMethodAlreadyExists")]
        public void TestFullExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }

    }
}