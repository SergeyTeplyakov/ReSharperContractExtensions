using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class FixMalformedContractsTests : QuickFixNet45TestBase<MalformedMethodContractQuickFix>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\MalformedContractQuickFixes"; }
        }

        [TestCase("FixIfThrowInIteratorBlock.cs")]
        [TestCase("FixIfThrowInAsyncMethod.cs")]
        public void Test_Fix_Precondition_In_AsyncMethod_Or_Iterator_Block(string fileName)
        {
            DoTestFiles(fileName);
        }

        [Test]
        [TestCase("FixMoveAssignmentAssertAssume.cs")]
        [TestCase("FixWithDifferentBlocks.cs")]
        [TestCase("FixWithErrorsAndWarnings.cs")]
        public void Test_Quick_Fix(string fileName)
        {
            DoTestFiles(fileName);
        }

        [TestCase("FixRequiresAfterEnsures.cs")]
        [TestCase("FixRequiresBetweenEnsures.cs")]
        public void Test_Fix_Requires_Ensures_Order(string fileName)
        {
            DoTestFiles(fileName);
        }

        [TestCase("FixRequiresAndEnsuresAfterEndContractBlock1.cs")]
        [TestCase("FixRequiresAndEnsuresAfterEndContractBlock2.cs")]
        public void Test_Fix_Precondition_Postcondition_After_EndContractBlock(string fileName)
        {
            DoTestFiles(fileName);
        }

        [TestCase("FixRemoveRedundantEndContractBlock.cs")]
        public void Test_Fix_For_Duplicated_EndContractBlock(string fileName)
        {
            DoTestFiles(fileName);
        }
    }
}