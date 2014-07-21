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

        [Test]
        [TestCase("FixMoveAssignmentAssertAssume.cs")]
        [TestCase("FixWithDifferentBlocks.cs")]
        [TestCase("FixWithErrorsAndWarnings.cs")]
        public void Test_Quick_Fix(string fileName)
        {
            DoTestFiles(fileName);
        }

    }
}