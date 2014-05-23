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

        [TestCase("ExecutionWithAbstractClass")]
        [TestCase("ExecutionWithInterface")]

        [TestCase("ExecutionWithExistingUsing")]
        [TestCase("ExecutionWithGenerics")]
        [TestCase("ExecutionWithRequires")]
        public void TestExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}