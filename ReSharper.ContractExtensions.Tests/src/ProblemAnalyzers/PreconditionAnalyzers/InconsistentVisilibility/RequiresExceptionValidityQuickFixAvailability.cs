using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class RequiresExceptionValidityQuickFixAvailability
        : QuickFixAvailabilityTestBase<RequiresExceptionValidityHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\RequiresExceptionValidity"; }
        }

        [TestCase("FixAvailableForMissedStringCtor.cs")]
        [TestCase("FixAvailableForMissedStringStringCtor.cs")]
        [TestCase("FixAvailableForMissedStringStringCtor2.cs")]
        [TestCase("FixUnavailableStringCtorExists.cs")]
        public void AvailabilityTest(string fileName)
        {
            DoTestFiles(fileName);
        }
    }
}