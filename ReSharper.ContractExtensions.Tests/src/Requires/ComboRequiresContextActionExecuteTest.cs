using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class ComboRequiresContextActionExecuteTest : CSharpContextActionExecuteTestBase<ComboRequiresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "ComboRequires"; }
        }

        [TestCase("ExecutionForAbstractMethod")]
        [TestCase("ExecutionForInterface")]
        [TestCase("ExecutionForAbstractProperty")]
        [TestCase("ExecutionForPartiallyDefinedContract")]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}