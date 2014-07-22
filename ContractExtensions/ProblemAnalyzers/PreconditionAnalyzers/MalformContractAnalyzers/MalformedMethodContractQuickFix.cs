using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    [QuickFix]
    public sealed class MalformedMethodContractQuickFix : QuickFixBase
    {
        private readonly MalformedContractFix _malformedContractFix;
        private readonly bool _isFixable;

        private MalformedMethodContractQuickFix(IMalformedMethodErrorHighlighting highlighting)
        {
            Contract.Requires(highlighting != null);

            _malformedContractFix = MalformedContractFix.TryCreate(highlighting.CurrentStatement, highlighting.ValidatedContractBlock);
        }

        public MalformedMethodContractQuickFix(MalformedMethodContractErrorHighlighting errorHighlighting)
            : this(errorHighlighting as IMalformedMethodErrorHighlighting)
        {
        }

        public MalformedMethodContractQuickFix(MalformedMethodContractWarningHighlighting warningHighlighting)
            : this(warningHighlighting as IMalformedMethodErrorHighlighting)
        {}

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_malformedContractFix != null);
            return _malformedContractFix.ExecuteFix();
        }

        public override string Text
        {
            get
            {
                Contract.Assert(_malformedContractFix != null);
                return _malformedContractFix.FixName;
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _malformedContractFix != null;
        }

        // Move this stuff to strategy!!!
        private abstract class MalformedContractFix
        {
            protected readonly ValidationResult _currentStatement;
            protected readonly ValidatedContractBlock _validatedContractBlock;

            protected MalformedContractFix(ValidationResult currentStatement, ValidatedContractBlock validatedContractBlock)
            {
                Contract.Requires(currentStatement != null);
                Contract.Requires(validatedContractBlock != null);

                _currentStatement = currentStatement;
                _validatedContractBlock = validatedContractBlock;
            }

            public static MalformedContractFix TryCreate(ValidationResult currentStatement, ValidatedContractBlock validatedContractBlock)
            {
                if (MoveStatementsOutOfTheContractBlockFix.IsFixableCore(currentStatement))
                    return new MoveStatementsOutOfTheContractBlockFix(currentStatement, validatedContractBlock);

                if (ArrangeRequiresAndEnsuresFix.IsFixableCore(currentStatement))
                    return new ArrangeRequiresAndEnsuresFix(currentStatement, validatedContractBlock);

                return null;
            }

            public Action<ITextControl> ExecuteFix()
            {
                return DoExecuteFix(_validatedContractBlock.ValidationResults.Where(IsFixable).ToList());
            }

            protected ICSharpStatement GetLastStatementInContractBlock()
            {
                return _validatedContractBlock.ContractBlock.Last().CSharpStatement;
            }
            protected abstract Action<ITextControl> DoExecuteFix(IList<ValidationResult> toFix);

            public abstract string FixName { get; }
            protected abstract bool IsFixable(ValidationResult validationResult);

        }

        private sealed class MoveStatementsOutOfTheContractBlockFix : MalformedContractFix
        {
            public MoveStatementsOutOfTheContractBlockFix(ValidationResult currentStatement, ValidatedContractBlock validatedContractBlock) 
                : base(currentStatement, validatedContractBlock)
            {}

            public static bool IsFixableCore(ValidationResult validationResult)
            {
                return validationResult.Match(
                    _ => false,
                    
                    error => error.Error == MalformedContractError.AssignmentInContractBlock ||
                             error.Error == MalformedContractError.AssertOrAssumeInContractBlock ||
                             error.Error == MalformedContractError.VoidReturnMethodCall,
                    
                    warning => warning.Warning == MalformedContractWarning.NonVoidReturnMethodCall);
            }

            protected override Action<ITextControl> DoExecuteFix(IList<ValidationResult> statementsToFix)
            {
                foreach (var s in statementsToFix)
                {
                    s.Statement.DetachFromParent();
                }

                var lastContractStatement = GetLastStatementInContractBlock();

                // curent statement could be changed uring our movements,
                // if so we'll use updated statement
                var updatedCurrentStatement =
                    lastContractStatement.AddStatementsAfter(statementsToFix.Select(s => s.Statement), 
                        _currentStatement.Statement);

                return textControl => textControl.Caret.MoveTo(updatedCurrentStatement ?? _currentStatement.Statement);
            }

            public override string FixName
            {
                get { return "Move statement(s) outside the contract section"; }
            }

            protected override bool IsFixable(ValidationResult validationResult)
            {
                return IsFixableCore(validationResult);
            }
        }

        private sealed class ArrangeRequiresAndEnsuresFix : MalformedContractFix
        {
            public ArrangeRequiresAndEnsuresFix(ValidationResult currentStatement, ValidatedContractBlock validatedContractBlock) 
                : base(currentStatement, validatedContractBlock)
            {}

            public static bool IsFixableCore(ValidationResult validationResult)
            {
                return validationResult.Match(
                    _ => false,
                    error => error.Error == MalformedContractError.RequiresAfterEnsures,
                    _ => false);
            }

            protected override Action<ITextControl> DoExecuteFix(IList<ValidationResult> statementsToFix)
            {
                var contractStatements = _validatedContractBlock.ContractBlock.Where(ps => ps.ContractStatement != null).ToList();

                var postconditions =
                    contractStatements.Where(x => x.ContractStatement.IsPostcondition)
                        .Select(x => x.CSharpStatement)
                        .ToList();

                // Removing all postconditions first
                foreach (var s in postconditions)
                {
                    s.DetachFromParent();
                }

                // And adding them back after last precondition
                var lastPrecondition = contractStatements.Last(s => s.ContractStatement.IsPrecondition).CSharpStatement;
                
                var updatedCurrentStatement = lastPrecondition.AddStatementsAfter(
                    postconditions, _currentStatement.Statement);

                return textControl => textControl.Caret.MoveTo(updatedCurrentStatement ?? _currentStatement.Statement);
            }

            public override string FixName
            {
                get { return "Arrange Contract.Requires/Ensures statement(s) in the contract section"; }
            }

            protected override bool IsFixable(ValidationResult validationResult)
            {
                return IsFixableCore(validationResult);
            }
        }
    }
}