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
        public const string Id = "MalformedMethodContractWarningHighlighting";
        private string _toolTip;

        public MalformedMethodContractWarningHighlighting(MalformedContractWarning warning, string contractMethod,
            ICSharpFunctionDeclaration functionDeclaration, IList<ProcessedStatement> contractBlock, ICSharpStatement failedStatement)
        {
            Contract.Requires(!string.IsNullOrEmpty(contractMethod));
            Contract.Requires(functionDeclaration != null);
            Contract.Requires(contractBlock != null);
            Contract.Requires(failedStatement != null);

            _toolTip = warning.GetErrorText(contractMethod);
            Warning = warning;
            FunctionDeclaration = functionDeclaration;
            ContractBlock = contractBlock;
            FailedStatement = failedStatement;
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
        public MalformedContractWarning Warning { get; private set; }
        public ICSharpFunctionDeclaration FunctionDeclaration { get; private set; }
        public IList<ProcessedStatement> ContractBlock { get; private set; }
        public ICSharpStatement FailedStatement { get; private set; }
    }
}