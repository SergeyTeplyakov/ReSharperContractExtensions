using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;

namespace ReSharper.ContractExtensions.Tests.Postconditions
{
        [TestFixture]
    public class EnsuresContextActionExecuteForIndexersTest : CSharpContextActionExecuteTestBase<EnsuresContextAction>
    {
        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/Ensures/IndexerFixes"; }
        }

        protected override string ExtraPath
        {
            get { return "Ensures\\IndexerFixes"; }
        }

        [TestCase("ExecuteOnGetter")]
        [TestCase("ExecuteOnIndexerDeclaration")]
        [TestCase("ExecuteOnReturn")]
        public void TestExecution(string testStr)
        {
            DoOneTest(testStr);
        }
    }

    [TestFixture]
    public class EnsuresContextActionExecuteTest : CSharpContextActionExecuteTestBase<EnsuresContextAction>
    {
        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/Ensures"; }
        }

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