using System;
using System.Diagnostics.Contracts;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings.Storage.Persistence;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContractsEx.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    [QuickFix]
    public sealed class MalformedContractStatementQuickFix : QuickFixBase
    {
        private readonly IMalformedContractStatementFix _fix;
        private readonly ValidationResult _validationResult;
        private readonly CodeContractStatement _contractStatement;

        public MalformedContractStatementQuickFix(MalformedContractStatementErrorHighlighting highlighting)
        {
            Contract.Requires(highlighting != null);

            _fix = MalformedContractStatementFix.TryCreate(highlighting.ValidationResult,
                highlighting.ProcessedStatement);
            _validationResult = highlighting.ValidationResult;
            _contractStatement = highlighting.ProcessedStatement;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            _fix.Apply(_validationResult, _contractStatement);
            return null;
        }

        public override string Text
        {
            get
            {
                Contract.Assert(_fix != null);
                return _fix.GetText();
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _fix != null;
        }
    }
}