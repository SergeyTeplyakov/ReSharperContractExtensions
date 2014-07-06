using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class RedundantRequiresCheckerTests : CSharpHighlightingTestBase<RedundantRequiresCheckerHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers"; }
        }

        [Test]
        [TestCase("WarnForCanBeNullParam.cs")]
        public void Debug(string testName)
        {
            DoTestSolution(testName);
        }

        [Test]
        [TestCase("NoWarning.cs")]
        [TestCase("NoWarningForPartiallyDefinedDefault.cs")]
        [TestCase("WarnForCanBeNullParam.cs")]
        [TestCase("WarnForNullableDefault.cs")]
        [TestCase("WarnForNullableDefaultWithMethodCall.cs")]
        public void Test(string testName)
        {
            DoTestSolution(testName);
        }

    }
}