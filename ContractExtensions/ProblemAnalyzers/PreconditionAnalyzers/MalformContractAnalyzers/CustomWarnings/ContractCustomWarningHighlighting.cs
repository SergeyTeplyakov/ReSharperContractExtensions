using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
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

        internal static ContractCustomWarningHighlighting Create(CustomWarningValidationResult warning,
            ValidatedContractBlock contractBlock)
        {
            switch (warning.Warning)
            {
                case MalformedContractCustomWarning.PreconditionInAsyncMethod:
                    return new PreconditionInAsyncMethodHighlighting(warning, contractBlock);
                case MalformedContractCustomWarning.PreconditionInMethodWithIteratorBlock:
                    return new PreconditionInMethodWithIteratorBlockHighlighing(warning, contractBlock);
                default:
                    return new ContractCustomWarningHighlighting(warning, contractBlock);
            }
        }

        internal ContractCustomWarningHighlighting(CustomWarningValidationResult warning, ValidatedContractBlock contractBlock)
        {
            Contract.Requires(warning != null);
            Contract.Requires(contractBlock != null);

            _toolTip = warning.GetErrorText();
            _validationResult = warning;
            _contractBlock = contractBlock;
            MethodName = warning.GetEnclosingMethodName();
            
        }

        public bool IsValid()
        {
            return true;
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