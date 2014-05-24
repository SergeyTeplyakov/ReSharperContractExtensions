using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;

namespace ReSharper.ContractExtensions.Tests.ContractFor
{
    [TestFixture]
    public class AddContractForContextActionExecuteTest : CSharpContextActionExecuteTestBase<AddContractForContextAction>
    {

        protected override string ExtraPath
        {
            get { return "ContractFor"; }
        }

        [TestCase("ExecutionForInterface")]
        [TestCase("ExecutionForEmptyInterface")]
        [TestCase("ExecutionForClass")]
        [TestCase("ExecutionForEmptyClass")]

        [TestCase("ExecutionForPartiallyDefinedClass")]
        [TestCase("ExecutionForPartiallyDefinedInterface")]
        public void TestExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}