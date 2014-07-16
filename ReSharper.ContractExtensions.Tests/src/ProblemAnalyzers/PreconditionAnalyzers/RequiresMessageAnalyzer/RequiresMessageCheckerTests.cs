using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class RequiresMessageCheckerTests : CSharpHighlightingTestBase<InvalidRequiresMessageHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\InvalidRequiresMessageChecker"; }
        }

        public void Debug(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("NoWarningConstant.cs")]
        [TestCase("NoWarningStaticField.cs")]
        [TestCase("NoWarningStaticProperty.cs")]
        [TestCase("NoWarningStringLiteral.cs")]
        public void Test(string testName)
        {
            DoTestSolution(testName);
        }



    }
}