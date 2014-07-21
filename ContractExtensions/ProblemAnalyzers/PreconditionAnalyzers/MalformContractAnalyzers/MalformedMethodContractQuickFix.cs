using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    [QuickFix]
    public sealed class MalformedMethodContractQuickFix : QuickFixBase
    {
        private readonly IMalformedMethodErrorHighlighting _highlighting;
        private readonly bool _isFixable;

        public MalformedMethodContractQuickFix(MalformedMethodContractErrorHighlighting errorHighlighting)
        {
            Contract.Requires(errorHighlighting != null);

            _highlighting = errorHighlighting;
            _isFixable = IsErrorFixable(errorHighlighting.Error);
        }

        public MalformedMethodContractQuickFix(MalformedMethodContractWarningHighlighting warningHighlighting)
        {
            Contract.Requires(warningHighlighting != null);

            _highlighting = warningHighlighting;
            _isFixable = IsWarningFixable(warningHighlighting.Warning);
        }

        private static bool IsFixable(ValidationResult validationResult)
        {
            if (validationResult.ErrorType == ErrorType.CodeContractError)
                return IsErrorFixable(validationResult.MalformedContractError);

            if (validationResult.ErrorType == ErrorType.CodeContractWarning)
                return IsWarningFixable(validationResult.MalformedContractWarning);

            return false;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var statements =
                ContractBlockValidator.ValidateContractBlockStatements(_highlighting.ContractBlock)
                    .Where(IsFixable).ToList();

            foreach (var s in statements)
            {
                var localParent = BlockNavigator.GetByStatement(s.Statement);
                Contract.Assert(localParent != null, "Can't find parent for the statement!");

                localParent.RemoveStatement(s.Statement);
            }

            var lastContractStatement = _highlighting.ContractBlock.Last().CSharpStatement;

            var lastContractParent = BlockNavigator.GetByStatement(lastContractStatement);
            Contract.Assert(lastContractParent != null, "Can't find parent for the last contract statement.");

            ICSharpStatement firstStatement = null;
            foreach (var s in statements)
            {
                lastContractStatement = lastContractParent.AddStatementAfter(s.Statement, lastContractStatement);

                if (firstStatement == null)
                    firstStatement = lastContractStatement;
            }
            Contract.Assert(firstStatement != null);

            return textControl =>
            {
                textControl.Caret.MoveTo(firstStatement.GetNavigationRange().TextRange.StartOffset,
                CaretVisualPlacement.Generic);
            };
        }

        public override string Text
        {
            get
            {
                return "Move statements outside the contract section";
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _isFixable;
        }

        private static bool IsErrorFixable(MalformedContractError error)
        {
            return error == MalformedContractError.AssignmentInContractBlock ||
                   error == MalformedContractError.AssertOrAssumeInContractBlock ||
                   error == MalformedContractError.VoidReturnMethodCall;
        }

        private static bool IsWarningFixable(MalformedContractWarning warning)
        {
            return warning == MalformedContractWarning.NonVoidReturnMethodCall;
        }
    }
}