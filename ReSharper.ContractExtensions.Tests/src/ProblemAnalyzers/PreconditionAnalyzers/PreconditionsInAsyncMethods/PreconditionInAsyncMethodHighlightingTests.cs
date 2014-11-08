using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class PreconditionInAsyncMethodHighlightingTests : CSharpHighlightingTestBase<PreconditionInAsyncMethodHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\PreconditionsInAsyncMethods"; }
        }

        [TestCase("NoWarning.cs")]
        public void Test_No_Warning(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("Warnings.cs")]
        public void Test_Warnings(string testName)
        {
            DoTestSolution(testName);
        }
    }
}