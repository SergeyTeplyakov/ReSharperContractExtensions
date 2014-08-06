using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PostconditionAnalyzers
{
    [TestFixture]
    public class PostconditionErrorHighlightingTests : CSharpHighlightingTestBase<CodeContractErrorHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\PostconditionAnalyzers"; }
        }

        [TestCase("ContractResultAnalyzer.cs")]
        public void Test_Contract_Result(string testName)
        {
            DoTestSolution(testName);
        }
    }
}