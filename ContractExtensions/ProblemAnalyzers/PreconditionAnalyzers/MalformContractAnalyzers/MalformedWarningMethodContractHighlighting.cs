using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(MalformedWarningMethodContractHighlighting.Id,
  null,
  HighlightingGroupIds.CompilerWarnings,
  MalformedWarningMethodContractHighlighting.Id,
  "Warn for malformed method contract",
  Severity.WARNING,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public sealed class MalformedWarningMethodContractHighlighting : IHighlighting
    {
        private readonly string _additionalMessage;
        public const string Id = "MalformedWarningMethodContractHighlighting";
        private const string _toolTipBase = "Malformed contract. ";

        public MalformedWarningMethodContractHighlighting(string additionalMessage)
        {
            _additionalMessage = additionalMessage;
            Contract.Requires(!string.IsNullOrEmpty(additionalMessage));
        }

        public bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return _toolTipBase + _additionalMessage; }
        }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }
    }
}