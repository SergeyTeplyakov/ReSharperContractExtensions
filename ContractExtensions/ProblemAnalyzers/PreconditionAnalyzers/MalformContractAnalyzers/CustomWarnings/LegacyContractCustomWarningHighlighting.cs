using System.Diagnostics.Contracts;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(LegacyContractCustomWarningHighlighting.InternalId,
  null,
  HighlightingGroupIds.CodeSmell,
  LegacyContractCustomWarningHighlighting.InternalId,
  "Warn for malformed contract statement",
  Severity.WARNING,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(InternalId, CSharpLanguage.Name)]
    public class LegacyContractCustomWarningHighlighting : IHighlighting, ICodeContractFixableIssue
    {
        private readonly ValidationResult _validationResult;
        private readonly ValidatedContractBlock _contractBlock;
        // It seems that using Id field in the base class led to an error (https://youtrack.jetbrains.com/issue/RSPL-6587)
        public const string InternalId = "Custom Warning for legacy contracts";
        private readonly string _toolTip;
        private readonly DocumentRange _range;

        internal static LegacyContractCustomWarningHighlighting Create(ICSharpFunctionDeclaration element, CustomWarningValidationResult warning, ValidatedContractBlock contractBlock)
        {
            switch (warning.Warning)
            {
                case MalformedContractCustomWarning.PreconditionInAsyncMethod:
                    return new PreconditionInAsyncMethodHighlighting(element, warning, contractBlock);
                case MalformedContractCustomWarning.PreconditionInMethodWithIteratorBlock:
                    return new PreconditionInMethodWithIteratorBlockHighlighing(element, warning, contractBlock);
                default:
                    return new LegacyContractCustomWarningHighlighting(element, warning, contractBlock);
            }
        }

        internal LegacyContractCustomWarningHighlighting(ICSharpFunctionDeclaration element, CustomWarningValidationResult warning, ValidatedContractBlock contractBlock)
        {
            Contract.Requires(warning != null);
            Contract.Requires(contractBlock != null);

            _toolTip = warning.GetErrorText();
            _range = element.GetHighlightingRange();
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