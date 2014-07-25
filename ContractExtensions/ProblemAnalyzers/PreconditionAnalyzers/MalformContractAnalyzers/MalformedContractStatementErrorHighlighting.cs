using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using ReSharper.ContractExtensions.ContractsEx.Statements;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(MalformedContractStatementErrorHighlighting.Id,
  null,
  HighlightingGroupIds.CompilerWarnings,
  MalformedContractStatementErrorHighlighting.Id,
  "Warn for malformed contract statement",
  Severity.ERROR,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{

    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public sealed class MalformedContractStatementErrorHighlighting : IHighlighting
    {
        private readonly ValidationResult _validationResult;
        private readonly CodeContractStatement _validatedStatement;
        public const string Id = "Malformed contract statement error highlighting";
        private readonly string _toolTip;

        internal MalformedContractStatementErrorHighlighting(CodeContractStatement validatedStatement, ValidationResult validationResult)
        {
            Contract.Requires(validatedStatement != null);
            Contract.Requires(validationResult != null);

            _validatedStatement = validatedStatement;
            _validationResult = validationResult;
            _toolTip = _validationResult.GetErrorText();
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