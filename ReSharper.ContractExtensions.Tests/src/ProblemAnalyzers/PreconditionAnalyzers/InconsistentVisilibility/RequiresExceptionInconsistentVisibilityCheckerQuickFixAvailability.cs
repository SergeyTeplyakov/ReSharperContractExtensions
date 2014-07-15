using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class RequiresExceptionInconsistentVisibilityCheckerQuickFixAvailability 
        : QuickFixAvailabilityTestBase<RequiresExceptionInconsistentVisibiityHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\RequiresExceptionVisibility"; }
        }

        [TestCase("ContractRequiresWarningForInnerException.cs")]
        [TestCase("ContractRequiresWarningForLessAccessibleException.cs")]
        public void AvailabilityTest(string fileName)
        {
            DoTestFiles(fileName);
        }
    }
}