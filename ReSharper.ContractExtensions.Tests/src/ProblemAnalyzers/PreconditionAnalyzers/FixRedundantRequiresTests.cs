using JetBrains.ReSharper.IntentionsTests;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class FixRedundantRequiresTests : QuickFixNet45TestBase<RedundantRequiresQuickFix>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers"; }
        }

        [TestCase("FixRequiresForNullableArgument.cs")]
        [Test]
        public void Test(string fileName)
        {
            DoTestFiles(fileName);
        }

    }
}