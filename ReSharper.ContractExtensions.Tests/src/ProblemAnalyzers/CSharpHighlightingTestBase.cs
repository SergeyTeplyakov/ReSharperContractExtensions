using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers
{
    public class CSharpHighlightingTestBase<THighlighting> : CSharpHighlightingTestNet45Base where THighlighting : IHighlighting
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting is THighlighting;
        }
    }
}