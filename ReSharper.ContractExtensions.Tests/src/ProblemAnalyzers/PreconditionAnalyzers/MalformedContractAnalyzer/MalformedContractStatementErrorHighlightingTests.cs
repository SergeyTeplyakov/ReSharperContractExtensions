using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class MalformedContractStatementErrorHighlightingTests 
        : CSharpHighlightingTestBase<ContractCustomWarningHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\MalformedContractStatements"; }
        }

        [TestCase("WarningDebug.cs")]
        public void Debug(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("WarningForContractInInnerStatement.cs")]
        public void Test_Highlighting(string testName)
        {
            DoTestSolution(testName);
        }
    }
}