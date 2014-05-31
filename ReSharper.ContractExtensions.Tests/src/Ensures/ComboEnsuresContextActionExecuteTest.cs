using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Ensures;

namespace ReSharper.ContractExtensions.Tests.Postconditions
{
    [TestFixture]
    public class ComboEnsuresContextActionExecuteTest : CSharpContextActionExecuteTestBase<ComboEnsuresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "ComboEnsures"; }
        }

        [TestCase("ExecutionForAbstractMethod")]
        [TestCase("ExecutionForInterface")]
        [TestCase("ExecutionForPartiallyDefinedContract")]
        [TestCase("ExecutionForAbstractProperty")]
        [TestCase("ExecutionForPropertyInTheInterface")]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }

    }
}