using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class FixRequiresExceptionValidityTests : QuickFixNet45TestBase<RequiresExceptionValidityQuickFix>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\RequiresExceptionValidity"; }
        }

        [TestCase("FixThatGenerateOneConstructor.cs")]
        [TestCase("FixThatGenerateTwoConstructors.cs")]
        public void Test(string fileName)
        {
            DoTestFiles(fileName);
        }

    }
}