using System.Diagnostics.Contracts;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(ContractCustomWarningHighlighting.Id,
  null,
  HighlightingGroupIds.CompilerWarnings,
  ContractCustomWarningHighlighting.Id,
  "Warn for malformed contract statement",
  Severity.WARNING,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public class ContractCustomWarningHighlighting : IHighlighting, ICodeContractFixableIssue
    {
        private readonly ValidationResult _validationResult;
        private readonly ValidatedContractBlock _contractBlock;
        public const string Id = "Custom Warning for contracts";
        private readonly string _toolTip;
        private readonly DocumentRange _range;

        internal static ContractCustomWarningHighlighting Create(ICSharpStatement statement, CustomWarningValidationResult warning, ValidatedContractBlock contractBlock)
        {
            switch (warning.Warning)
            {
                case MalformedContractCustomWarning.PreconditionInAsyncMethod:
                case MalformedContractCustomWarning.PreconditionInMethodWithIteratorBlock:
                    return null;
                default:
                    return new ContractCustomWarningHighlighting(statement, warning, contractBlock);
            }
        }

        internal ContractCustomWarningHighlighting(ICSharpStatement statement, CustomWarningValidationResult warning, ValidatedContractBlock contractBlock)
        {
            Contract.Requires(warning != null);
            Contract.Requires(contractBlock != null);
            Contract.Requires(_range != null);

            _range = statement.GetHighlightingRange();
            _toolTip = warning.GetErrorText();
            _validationResult = warning;
            _contractBlock = contractBlock;
            MethodName = warning.GetEnclosingMethodName();
            
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

        public string MethodName { get; private set; }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }

        public ValidationResult ValidationResult
        {
            get { return _validationResult; }
        }

        ValidatedContractBlock ICodeContractFixableIssue.ValidatedContractBlock
        {
            get { return _contractBlock; }
        }
    }
}