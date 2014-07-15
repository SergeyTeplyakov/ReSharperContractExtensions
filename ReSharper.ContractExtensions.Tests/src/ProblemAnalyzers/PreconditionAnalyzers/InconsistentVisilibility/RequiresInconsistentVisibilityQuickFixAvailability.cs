using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class RequiresInconsistentVisibilityQuickFixAvailability : QuickFixAvailabilityTestBase<RequiresInconsistentVisibiityHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\InconsistentVisibility"; }
        }

        [TestCase("FixAvailableForAnotherType.cs")]
        [TestCase("FixAvailableForMethodInTheBaseType.cs")]
        [TestCase("FixAvailableForMethodInTheSameType.cs")]
        [TestCase("FixAvailableForPrivateInnerType.cs")]
        [TestCase("FixAvailableForPropertyInTheSameType.cs")]
        [TestCase("FixUnavailableForProtectedContractHolder.cs")]
        [TestCase("FixAvailableForTypeVisibilityOnly.cs")]
        public void AvailabilityTest(string fileName)
        {
            DoTestFiles(fileName);
        }
    }
}