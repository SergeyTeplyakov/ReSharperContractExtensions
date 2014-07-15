using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class RequiresExceptionInconsistentVisibilityCheckerTests 
        : CSharpHighlightingTestBase<RequiresExceptionInconsistentVisibiityHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\RequiresExceptionVisibility"; }
        }

        public void Debug(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("ContractRequiresNoWarning.cs")]
        [TestCase("ContractRequiresNoWarningBuildInException.cs")]

        [TestCase("ContractRequiresWarningForInnerException.cs")]
        [TestCase("ContractRequiresWarningForLessAccessibleException.cs")]
        public void Test(string testName)
        {
            DoTestSolution(testName);
        }

    }
}