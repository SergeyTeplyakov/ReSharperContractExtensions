using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;
using ReSharper.ContractExtensions.ContextActions.EnumChecks;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class EnumCheckEnsuresContextActionExecutionTest : RequiresContextActionExecuteTestBase<EnumCheckEnsuresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "EnumCheckEnsures"; }
        }

        [TestCase("ExecutionForCustomEnum")]
        [TestCase("ExecutionForNullableCustomEnum")]
        [TestCase("ExecutionForDotNetEnum")]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }

    }
}