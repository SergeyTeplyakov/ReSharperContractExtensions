using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions.Statements;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(MalformedContractErrorHighlighting.Id,
  null,
  HighlightingGroupIds.CompilerWarnings,
  MalformedContractErrorHighlighting.Id,
  "Warn for malformed contract statement",
  Severity.ERROR,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{

    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public sealed class MalformedContractErrorHighlighting : IHighlighting
    {
        private readonly ValidationResult _validationResult;
        private readonly CodeContractStatement _validatedStatement;
        public const string Id = "Malformed contract statement error highlighting";
        private readonly string _toolTip;
        private readonly DocumentRange _range;

        internal MalformedContractErrorHighlighting(ICSharpStatement statement, CodeContractStatement validatedStatement, ValidationResult validationResult)
        {
            Contract.Requires(validatedStatement != null);
            Contract.Requires(validationResult != null);

            _range = statement.GetHighlightingRange();
            _validatedStatement = validatedStatement;
            _validationResult = validationResult;
            _toolTip = _validationResult.GetErrorText();
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

        internal ValidationResult ValidationResult
        {
            get
            {
                Contract.Ensures(Contract.Result<ValidationResult>() != null);
                return _validationResult;
            }
        }

        internal CodeContractStatement ProcessedStatement
        {
            get
            {
                Contract.Ensures(Contract.Result<CodeContractStatement>() != null);
                return _validatedStatement;
            }
        }
    }
}