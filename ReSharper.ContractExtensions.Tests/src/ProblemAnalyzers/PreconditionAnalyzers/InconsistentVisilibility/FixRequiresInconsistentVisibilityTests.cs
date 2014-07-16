using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class FixInconsistentVisibilityTests : QuickFixNet45TestBase<RequiresInconsistentVisibiityQuickFix>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\InconsistentVisibility"; }
        }

        [TestCase("QuickFixForAnotherType.cs")]
        [TestCase("QuickFixForMethodInTheSameType.cs")]
        [TestCase("QuickFixFormMethodInTheBaseType.cs")]
        [TestCase("QuickFixForPrivateInnerType.cs")]
        [TestCase("QuickFixForTypeVisibilityOnly.cs")]
        public void Test(string fileName)
        {
            DoTestFiles(fileName);
        }

    }
}