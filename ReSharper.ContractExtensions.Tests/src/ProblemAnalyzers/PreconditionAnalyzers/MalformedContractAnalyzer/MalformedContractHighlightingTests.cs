using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class MalformedContractHighlightingTests : CSharpHighlightingTestBase<MalformedMethodContractHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\MalformedContracts"; }
        }

        public void Debug(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("NoWarning.cs")]
        [TestCase("NoWarningForVoidMethodWithArgumentValidation.cs")]

        [TestCase("WarningForInstanceVoidMethod.cs")]
        [TestCase("WarningForInstanceMethodAndContractInInnerBlock.cs")]
        [TestCase("WarningForStaticVoidMethodBeforeEnsures.cs")]
        [TestCase("WarningOnConsoleWriteLineBeforeEndContractBlock.cs")]

        [TestCase("WarningForTwoMethods.cs")]
        [TestCase("WarningOnConsoleWriteLineBeforeEndContractBlock.cs")]
        [TestCase("WarningForTwoInterplacedMethods.cs")]
        public void Test(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("WarnForAssertOrAssumeInContractBlock.cs")]
        public void Test_Assert_Assume(string testName)
        {
            DoTestSolution(testName);
        }



    }
}