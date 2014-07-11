using JetBrains.ReSharper.IntentionsTests;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class ContractPublicPropertyNameQuickFixExecutionTests 
        : QuickFixNet45TestBase<ContractPublicPropertyNameQuickFix>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\ContractPublicPropertyNameCheckers"; }
        }

        [TestCase("FixPropertyVisibility.cs")]
        [TestCase("FixByAddingPublicProperty.cs")]
        public void Test(string fileName)
        {
            DoTestFiles(fileName);
        }

    }
}