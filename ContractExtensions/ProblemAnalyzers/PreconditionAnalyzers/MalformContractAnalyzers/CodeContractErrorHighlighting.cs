using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(CodeContractErrorHighlighting.Id,
  null,
  HighlightingGroupIds.CompilerWarnings,
  CodeContractErrorHighlighting.Id,
  "Warn for malformed method contract",
  Severity.ERROR,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public sealed class CodeContractErrorHighlighting : IHighlighting, ICodeContractFixableIssue
    {
        private readonly CodeContractErrorValidationResult _error;
        private readonly ValidatedContractBlock _contractBlock;
        public const string Id = "Malformed method contract error highlighting";
        private readonly string _toolTip;

        internal CodeContractErrorHighlighting(CodeContractErrorValidationResult error, ValidatedContractBlock contractBlock)
        {
            Contract.Requires(error != null);
            Contract.Requires(contractBlock != null);

            _error = error;
            _contractBlock = contractBlock;
            _toolTip = error.GetErrorText();
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

        ValidationResult ICodeContractFixableIssue.ValidationResult
        {
            get { return _error; }
        }

        ValidatedContractBlock ICodeContractFixableIssue.ValidatedContractBlock
        {
            get { return _contractBlock; }
        }
    }
}