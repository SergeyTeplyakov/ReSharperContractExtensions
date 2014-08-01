using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class MalformedContractHighlightingTests : CSharpHighlightingTestBase<MalformedMethodContractErrorHighlighting>
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
        public void Test_No_Warning(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("WarningForInstanceVoidMethod.cs")]
        [TestCase("WarningForInstanceVoidMethodBeforeEnsuresOnThrow.cs")]
        [TestCase("WarningForInstanceMethodAndContractInInnerBlock.cs")]
        [TestCase("WarningForStaticVoidMethodBeforeEnsures.cs")]
        [TestCase("WarningOnConsoleWriteLineBeforeEndContractBlock.cs")]

        [TestCase("WarningForTwoMethods.cs")]
        [TestCase("WarningOnConsoleWriteLineBeforeEndContractBlock.cs")]
        [TestCase("WarningForTwoInterplacedMethods.cs")]
        public void Test_Void_Return_Method_In_Contract_Block(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("WarnForAssertOrAssumeInContractBlock.cs")]
        public void Test_Assert_Assume(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("WarningForAssignmentInContractBlock.cs")]
        public void Test_Assignment_In_Contract_Block(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("WarningForRequiresAfterEnsures.cs")]
        [TestCase("WarningForRequiresAfterEnsuresOnThrow.cs")]
        public void Test_Precondition_After_Postcondition(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("WarningForEndContractBlockBeforeEnsures.cs")]
        [TestCase("WarningForEndContractBlockBeforeRequires.cs")]
        public void Test_Contract_Statements_After_EndContractBlock(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("SeveralWarnings.cs")]
        public void Test_Several_Warnings(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("WarningForDuplicatedEndContractBlock.cs")]
        public void Test_For_Duplicate_EndContractBlock_Call(string testName)
        {
            DoTestSolution(testName);
        }
    }
}