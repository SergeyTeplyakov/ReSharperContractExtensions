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
    public interface IMalformedMethodErrorHighlighting
    {
        IList<ProcessedStatement> ContractBlock { get; }
        ICSharpStatement FailedStatement { get; }
        ICSharpFunctionDeclaration FunctionDeclaration { get; }
    }

    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public sealed class MalformedMethodContractErrorHighlighting : IHighlighting, IMalformedMethodErrorHighlighting
    {
        public const string Id = "MalformedMethodContractErrorHighlighting";
        private string _toolTip;

        internal MalformedMethodContractErrorHighlighting(MalformedContractError error, string contractMethodName,
            ICSharpFunctionDeclaration functionDeclaration, IList<ProcessedStatement> contractBlock, ICSharpStatement failedStatement)
        {
            Contract.Requires(!string.IsNullOrEmpty(contractMethodName));
            Contract.Requires(functionDeclaration != null);
            Contract.Requires(contractBlock != null);
            Contract.Requires(failedStatement != null);

            _toolTip = error.GetErrorText(contractMethodName);
            Error = error;
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
        public MalformedContractError Error { get; private set; }
        public ICSharpFunctionDeclaration FunctionDeclaration { get; private set; }
        public IList<ProcessedStatement> ContractBlock { get; private set; }
        public ICSharpStatement FailedStatement { get; private set; }
    }
}