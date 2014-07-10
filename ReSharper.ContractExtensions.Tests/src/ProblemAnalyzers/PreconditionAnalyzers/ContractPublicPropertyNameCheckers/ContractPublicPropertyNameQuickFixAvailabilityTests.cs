using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class ContractPublicPropertyNameQuickFixAvailabilityTests : QuickFixAvailabilityTestBase<ContractPublicPropertyNameHighlighing>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\ContractPublicPropertyNameCheckers"; }
        }

        [TestCase("ChangeVisibilityAvailableForField.cs")]
        [TestCase("ChangeVisibilityAvailableForProperty.cs")]

        [TestCase("GeneratePropertyAvailable.cs")]
        public void AvailabilityTest(string fileName)
        {
            DoTestFiles(fileName);
        }

    }
}