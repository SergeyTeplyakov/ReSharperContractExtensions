using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class ComboMethodRequiresContextActionExecuteTest : RequiresContextActionExecuteTestBase<ComboMethodRequiresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "ComboRequiresForMethod"; }
        }

        [TestCase("ExecutionForInterface")]
        [TestCase("ExecutionOnAbstractMethodWithContract")]
        [TestCase("ExecutionOnAbstractMethodWithoutContract")]
        [TestCase("ExecutionOnNotAbstractMethod")]
        [TestCase("ExecutionOnSimpleMethodWithMixOfArguments")]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}