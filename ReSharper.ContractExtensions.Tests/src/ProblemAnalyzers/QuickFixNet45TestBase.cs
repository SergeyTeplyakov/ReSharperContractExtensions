using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
//using JetBrains.ReSharper.Intentions.Extensibility;
//using JetBrains.ReSharper.IntentionsTests;
using JetBrains.ReSharper.TestFramework;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers
{
    [TestNetFramework45]
    public abstract class QuickFixNet45TestBase<TQuickFix> : QuickFixTestBase<TQuickFix> where TQuickFix : QuickFixBase
    {}
}