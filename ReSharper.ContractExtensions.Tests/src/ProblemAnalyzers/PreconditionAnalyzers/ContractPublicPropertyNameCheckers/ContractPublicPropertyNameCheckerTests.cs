using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class ContractPublicPropertyNameCheckerTests : CSharpHighlightingTestBase<ContractPublicPropertyNameHighlighing>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\ContractPublicPropertyNameCheckers"; }
        }

        public void Debug(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("NoWarning.cs")]
        [TestCase("WarnForNonExistedProperty.cs")]
        [TestCase("WarnForNonPublicField.cs")]
        [TestCase("WarnForNonPublicProperty.cs")]
        public void Test_Warnings(string testName)
        {
            DoTestSolution(testName);
        }

    }
}