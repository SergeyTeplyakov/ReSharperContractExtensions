using System.Diagnostics.Contracts;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
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
        private readonly DocumentRange _range;

        internal CodeContractErrorHighlighting(ICSharpStatement statement, CodeContractErrorValidationResult error, ValidatedContractBlock contractBlock)
        {
            Contract.Requires(error != null);
            Contract.Requires(contractBlock != null);
            Contract.Requires(statement != null);

            _range = statement.GetHighlightingRange();
            _error = error;
            _contractBlock = contractBlock;
            _toolTip = error.GetErrorText();
        }

        public bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// Calculates range of a highlighting.
        /// </summary>
        public DocumentRange CalculateRange()
        {
            return _range;
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