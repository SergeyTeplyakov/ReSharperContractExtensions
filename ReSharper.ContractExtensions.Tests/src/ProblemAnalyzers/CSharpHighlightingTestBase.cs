using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.TestFramework;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers
{
    [TestReferences(@"%PRODUCT%\JetBrains.Annotations.dll")]
    public abstract class CSharpHighlightingTestBase<THighlighting> : CSharpHighlightingTestNet45Base where THighlighting : IHighlighting
    {
        //protected override  
        //protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        //{
        //    return highlighting is THighlighting;
        //}
    }
}