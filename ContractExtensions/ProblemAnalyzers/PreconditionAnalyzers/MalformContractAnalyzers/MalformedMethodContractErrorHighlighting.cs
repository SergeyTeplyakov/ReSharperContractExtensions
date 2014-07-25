using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Statements;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(MalformedMethodContractErrorHighlighting.Id,
  null,
  HighlightingGroupIds.CompilerWarnings,
  MalformedMethodContractErrorHighlighting.Id,
  "Warn for malformed method contract",
  Severity.ERROR,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal interface IMalformedMethodErrorHighlighting
    {
        ValidationResult CurrentStatement { get; }
        ValidatedContractBlock ValidatedContractBlock { get; }
    }

    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public sealed class MalformedMethodContractErrorHighlighting : IHighlighting, IMalformedMethodErrorHighlighting
    {
        private readonly CodeContractErrorValidationResult _error;
        private readonly ValidatedContractBlock _contractBlock;
        public const string Id = "Malformed method contract error highlighting";
        private readonly string _toolTip;

        internal MalformedMethodContractErrorHighlighting(CodeContractErrorValidationResult error, ValidatedContractBlock contractBlock)
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

        ValidationResult IMalformedMethodErrorHighlighting.CurrentStatement
        {
            get { return _error; }
        }

        ValidatedContractBlock IMalformedMethodErrorHighlighting.ValidatedContractBlock
        {
            get { return _contractBlock; }
        }
    }
}