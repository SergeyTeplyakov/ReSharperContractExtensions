using JetBrains.ReSharper.IntentionsTests;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class FixInconsistentVisibilityTests : QuickFixNet45TestBase<RequiresInconsistentVisibiityQuickFix>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers"; }
        }

        [TestCase("FixEnclosingMethodVisibility.cs")]
        [TestCase("FixEnclosingPropertyVisibility.cs")]
        public void Test(string fileName)
        {
            DoTestFiles(fileName);
        }

    }
}