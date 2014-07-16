using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class RequiresExceptionConstructorCheckerTests
        : CSharpHighlightingTestBase<RequiresExceptionValidityHighlighting>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"Daemon\PreconditionAnalyzers\RequiresExceptionValidity"; }
        }

        public void Debug(string testName)
        {
            DoTestSolution(testName);
        }

        [TestCase("NoWarningExceptionWithOneStringArgument.cs")]
        [TestCase("NoWarningExceptionWithTwoStringArguments.cs")]

        [TestCase("WarningLackOfExplicitCtor.cs")]
        public void Test(string testName)
        {
            DoTestSolution(testName);
        }

    }
}