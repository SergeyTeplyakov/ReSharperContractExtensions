using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class FixMalformedContractStatementssTests : QuickFixNet45TestBase<MalformedContractStatementQuickFix>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\MalformedContractStatements"; }
        }

        [TestCase("FixRemoveEndContractBlock.cs")]
        [TestCase("FixRemoveEndContractBlock2.cs")]
        public void Test_Quick_EndContractBlock(string fileName)
        {
            DoTestFiles(fileName);
        }

        [TestCase("FixChangePreconditionToAssert.cs")]
        [TestCase("FixChangePreconditionToAssertWithMessage.cs")]
        public void Test_Fix_Preconditions(string fileName)
        {
            DoTestFiles(fileName);
        }

        [TestCase("FixMovePostconditionToTheContractBlock.cs")]
        [TestCase("FixMovePostconditionToTheContractBlock2.cs")]
        [TestCase("FixMovePostconditionToTheContractBlock3.cs")]
        public void Test_Fix_Postcondition(string fileName)
        {
            DoTestFiles(fileName);
        }

        [TestCase("FixMoveContractsOutOfTheTryBlock.cs")]
        [TestCase("FixMoveContractsOutOfTheTryBlock2.cs")]
        [TestCase("FixMoveContractsOutOfTheTryBlock3.cs")]
        public void Test_Fix_For_Contract_In_Try_Block(string fileName)
        {
            DoTestFiles(fileName);
        }

    }
}