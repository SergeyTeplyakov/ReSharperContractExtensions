using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(MalformedMethodContractHighlighting.Id,
  null,
  HighlightingGroupIds.CompilerWarnings,
  MalformedMethodContractHighlighting.Id,
  "Warn for malformed method contract",
  Severity.ERROR,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public sealed class MalformedMethodContractHighlighting : IHighlighting
    {
        public const string Id = "MalformedMethodContractHighlighting";
        private const string _toolTipBase = "Malformed contract. Void-returned method call in the contract section";

        public MalformedMethodContractHighlighting()
        {
        }

        public bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return _toolTipBase; }
        }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }
    }
}