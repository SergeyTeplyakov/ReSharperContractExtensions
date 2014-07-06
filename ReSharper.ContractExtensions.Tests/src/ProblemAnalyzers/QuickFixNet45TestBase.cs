using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.IntentionsTests;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers
{
    [TestFixture]
    [TestNetFramework45]
    public class QuickFixNet45TestBase<TQuickFix> : QuickFixTestBase<TQuickFix> where TQuickFix : QuickFixBase
    {}
}