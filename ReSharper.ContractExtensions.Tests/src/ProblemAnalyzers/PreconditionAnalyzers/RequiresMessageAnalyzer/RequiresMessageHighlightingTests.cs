using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class RequiresMessageHighlightingTests : CSharpHighlightingTestBase<InvalidRequiresMessageHighlighting>
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

        [TestCase("WarningOnInstanceField.cs")]
        [TestCase("WarningOnInstanceProperty.cs")]
        [TestCase("WarningOnMethodCall.cs")]
        [TestCase("WarningOnPrivateStaticField.cs")]
        public void Test(string testName)
        {
            DoTestSolution(testName);
        }



    }
}