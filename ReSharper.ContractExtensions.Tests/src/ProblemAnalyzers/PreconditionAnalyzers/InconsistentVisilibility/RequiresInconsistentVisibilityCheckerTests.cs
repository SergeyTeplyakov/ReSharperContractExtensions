using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class RequiresInconsistentVisibilityCheckerTests : CSharpHighlightingTestBase<RequiresInconsistentVisibiityHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\IncosistentVisibilityChecker"; }
        }

        public void Debug(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("NoWarning.cs")]
        [TestCase("NoWarningWithContractPublicPropertyName.cs")]

        [TestCase("PublicPrivatePropertyWarning.cs")]
        [TestCase("WarningForPrivateInnerType.cs")]
        [TestCase("WarningForDifferentTypeVisibility.cs")]
        [TestCase("WarningFromPropertyPrecondition.cs")]
        [TestCase("PublicPrivateFieldWarning.cs")]

        [TestCase("PublicProtectedWarning.cs")]
        [TestCase("PublicInternalWarning.cs")]
        [TestCase("PublicPrivateWarning.cs")]
        public void Test(string testName)
        {
            DoTestSolution(testName);
        }

    }
}