using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;

namespace ReSharper.ContractExtensions.Tests.Postconditions
{
    [TestFixture]
    public class EnsuresContextActionExecuteTest : CSharpContextActionExecuteTestBase<EnsuresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "Ensures"; }
        }

        [TestCase("Execution")]
        [TestCase("ExecutionFull")]
        [TestCase("ExecutionWithExistingUsing")]
        [TestCase("ExecutionWithGenerics")]
        [TestCase("ExecutionWithRequires")]
        [Test]
        public void TestExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}