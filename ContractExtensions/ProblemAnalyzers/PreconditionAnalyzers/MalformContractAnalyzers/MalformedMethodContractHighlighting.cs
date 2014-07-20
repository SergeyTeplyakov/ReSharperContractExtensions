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
        private string _toolTip;

        internal MalformedMethodContractHighlighting(MalformedContractError error, string contractMethodName)
        {
            Contract.Requires(!string.IsNullOrEmpty(contractMethodName));

            _toolTip = GetErrorText(error, contractMethodName);
        }

        public bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return _toolTip; }
        }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }

        private static string GetErrorText(MalformedContractError error, string contractMethodName)
        {
            switch (error)
            {
                case MalformedContractError.VoidReturnMethodCall:
                    return string.Format("Malformed contract. Detected expression statement evaluated for side-effect in contracts of method '{0}'", 
                        contractMethodName);
                case MalformedContractError.AssertOrAssumeInContractBlock:
                    return string.Format("Contract.Assert/Contract.Assume cannot be used in contract section of method '{0}'. Use only Requires and Ensures",
                        contractMethodName);
                case MalformedContractError.AssignmentInContractBlock:
                    // Code Contract error: Malformed contract. Found Requires after assignment in method 'CodeContractInvestigations.MalformedContractErrors.AssignmentInContractBlock'.
                    return string.Format("Malformed contract. Assignment cannot be used in contract section of method '{0}'", 
                        contractMethodName);
                default:
                    throw new ArgumentOutOfRangeException("error");
            }
        }
    }
}