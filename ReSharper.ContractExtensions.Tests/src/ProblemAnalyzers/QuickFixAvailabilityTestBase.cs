using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Intentions.Test;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers
{
    [TestFixture]
    [TestNetFramework45]
    public class QuickFixAvailabilityTestBase<THighlighting> : QuickFixAvailabilityTestBase where THighlighting : IHighlighting
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile psiSourceFile)
        {
            return highlighting is THighlighting;
        }
    }
}