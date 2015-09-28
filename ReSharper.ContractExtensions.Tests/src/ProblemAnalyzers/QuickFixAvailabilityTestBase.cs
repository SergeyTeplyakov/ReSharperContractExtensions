using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers
{
    [TestNetFramework45]
    public abstract class QuickFixAvailabilityTestBase<THighlighting> : QuickFixAvailabilityTestBase where THighlighting : IHighlighting
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile psiSourceFile)
        {
            return highlighting is THighlighting;
        }
    }
}