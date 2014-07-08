using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class RequiresInconsistentVisibilityCheckerTests : CSharpHighlightingTestBase<RequiresInconsistentVisibiityHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\IncosistentVisibilityChecker"; }
        }

        [Test]
        public void Debug(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("PublicPrivatePropertyWarning.cs")]

        [TestCase("NoWarning.cs")]
        
        [TestCase("PublicProtectedWarning.cs")]
        [TestCase("PublicInternalWarning.cs")]
        [TestCase("PublicPrivateWarning.cs")]
        public void Test(string testName)
        {
            DoTestSolution(testName);
        }

    }
}