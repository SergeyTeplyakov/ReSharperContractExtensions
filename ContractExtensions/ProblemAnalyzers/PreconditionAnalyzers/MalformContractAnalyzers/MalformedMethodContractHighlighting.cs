using System;
using System.Diagnostics.Contracts;
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
        private readonly string _additionalMessage;
        public const string Id = "MalformedMethodContractHighlighting";
        private const string _toolTipBase = "Malformed contract. ";

        internal MalformedMethodContractHighlighting(MalformedContractError error, string contractMethodName)
        {
            Contract.Requires(!string.IsNullOrEmpty(contractMethodName));

            _additionalMessage = GetErrorText(error, contractMethodName);
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

        private static string GetErrorText(MalformedContractError error, string contractMethodName)
        {
            switch (error)
            {
                case MalformedContractError.VoidReturnMethodCall:
                    return string.Format("Detected expression statement evaluated for side-effect in contracts of method '{0}'", 
                        contractMethodName);
                default:
                    throw new ArgumentOutOfRangeException("error");
            }
        }
    }
}