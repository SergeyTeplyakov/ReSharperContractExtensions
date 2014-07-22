using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Statements;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(MalformedMethodContractWarningHighlighting.Id,
  null,
  HighlightingGroupIds.CompilerWarnings,
  MalformedMethodContractWarningHighlighting.Id,
  "Warn for malformed method contract",
  Severity.WARNING,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Shows warnings, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public sealed class MalformedMethodContractWarningHighlighting : IHighlighting, IMalformedMethodErrorHighlighting
    {
        private readonly CodeContractWarningValidationResult _warning;
        private readonly ValidatedContractBlock _contractBlock;
        public const string Id = "MalformedMethodContractWarningHighlighting";
        private string _toolTip;

        internal MalformedMethodContractWarningHighlighting(CodeContractWarningValidationResult warning, ValidatedContractBlock contractBlock)
        {
            Contract.Requires(warning != null);
            Contract.Requires(contractBlock != null);

            _warning = warning;
            _contractBlock = contractBlock;

            _toolTip = warning.GetErrorText();
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
            get { return _warning; }
        }

        ValidatedContractBlock IMalformedMethodErrorHighlighting.ValidatedContractBlock
        {
            get { return _contractBlock; }
        }

    }
}