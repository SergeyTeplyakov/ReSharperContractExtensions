using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Forms;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.ContractsEx.Assertions.Statements;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal abstract class MalformedContractFix
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

            if (MovePreconditionsAndPostconditionsBeforeEndContractBlock.IsFixableCore(currentStatement))
                return new MovePreconditionsAndPostconditionsBeforeEndContractBlock(currentStatement, validatedContractBlock);

            if (RemoveRedundantStatementFix.IsFixableCore(currentStatement))
                return new RemoveRedundantStatementFix(currentStatement, validatedContractBlock);

            if (FixContractResultTypeInContractEnsure.IsFixableCore(currentStatement))
                return new FixContractResultTypeInContractEnsure(currentStatement, validatedContractBlock);

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

    internal sealed class MoveStatementsOutOfTheContractBlockFix : MalformedContractFix
    {
        public MoveStatementsOutOfTheContractBlockFix(ValidationResult currentStatement, ValidatedContractBlock validatedContractBlock)
            : base(currentStatement, validatedContractBlock)
        { }

        public static bool IsFixableCore(ValidationResult validationResult)
        {
            return validationResult.Match(
                error => error.Error == MalformedContractError.AssignmentInContractBlock ||
                         error.Error == MalformedContractError.AssertOrAssumeInContractBlock ||
                         error.Error == MalformedContractError.VoidReturnMethodCall,

                warning => warning.Warning == MalformedContractWarning.NonVoidReturnMethodCall,

                _ => false);
        }

        protected override Action<ITextControl> DoExecuteFix(IList<ValidationResult> statementsToFix)
        {
            foreach (var s in statementsToFix)
            {
                s.Statement.RemoveOrReplaceByEmptyStatement();
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

    internal sealed class ArrangeRequiresAndEnsuresFix : MalformedContractFix
    {
        public ArrangeRequiresAndEnsuresFix(ValidationResult currentStatement, ValidatedContractBlock validatedContractBlock)
            : base(currentStatement, validatedContractBlock)
        { }

        public static bool IsFixableCore(ValidationResult validationResult)
        {
            return validationResult.Match(
                _ => false,
                error => error.Error == MalformedContractError.RequiresAfterEnsures,
                _ => false);
        }

        protected override Action<ITextControl> DoExecuteFix(IList<ValidationResult> statementsToFix)
        {
            var contractStatements = _validatedContractBlock.ContractBlock.Where(ps => ps.CodeContractStatement != null).ToList();

            var postconditions =
                contractStatements.Where(x => x.CodeContractStatement.IsPostcondition)
                    .Select(x => x.CSharpStatement)
                    .ToList();

            // Removing all postconditions first
            foreach (var s in postconditions)
            {
                s.RemoveOrReplaceByEmptyStatement();
            }

            // And adding them back after last precondition
            var lastPrecondition = contractStatements.Last(s => s.CodeContractStatement.IsPrecondition).CSharpStatement;

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

    internal sealed class MovePreconditionsAndPostconditionsBeforeEndContractBlock : MalformedContractFix
    {
        public MovePreconditionsAndPostconditionsBeforeEndContractBlock(ValidationResult currentStatement,
            ValidatedContractBlock validatedContractBlock)
            : base(currentStatement, validatedContractBlock)
        { }

        public static bool IsFixableCore(ValidationResult validationResult)
        {
            return validationResult.Match(
                _ => false,
                error => error.Error == MalformedContractError.ReqruiesOrEnsuresAfterEndContractBlock,
                _ => false);
        }

        protected override Action<ITextControl> DoExecuteFix(IList<ValidationResult> statementsToFix)
        {
            // This fix has following logic:
            // 1. Find first occurrance of the EndContracBlock statement (will be required later)
            // 2. Find all preconditions and postconditions after EndContractBlock
            // 3. Remove items from step 2
            // 4. Add items from step 2 and 3 before first statement found at step 1

            // Contract block could have different statements, not only code contract statements, but
            // now we're interested only in code contract statements!
            var codeContractStatements =
                _validatedContractBlock.ContractBlock.Where(p => p.CodeContractStatement != null).ToList();

            // Looking for the first EndContractBlock (theoretically there could be more then one!)
            var firstEndContractBlockIndex =
                codeContractStatements.FirstIndexOf(
                    ps => ps.CodeContractStatement.StatementType == CodeContractStatementType.EndContractBlock);

            Contract.Assert(firstEndContractBlockIndex != -1, "Contract block should have EndContractBlock statement!");

            var preconditionsOrPostconditions =
                codeContractStatements
                .Skip(firstEndContractBlockIndex)
                .Where(ps => ps.CodeContractStatement.IsPrecondition || ps.CodeContractStatement.IsPostcondition)
                .Select(s => s.CSharpStatement)
                .ToList();

            foreach (var s in preconditionsOrPostconditions)
            {
                s.RemoveOrReplaceByEmptyStatement();
            }

            // Then we'll find first contract block
            var firstEndContractBlock = codeContractStatements[firstEndContractBlockIndex].CSharpStatement;

            // And will add already found preconditions and postconditions before that statement
            // (in reverse order, because this will preserve original order)
            var updatedCurrentStatement =
                firstEndContractBlock.AddStatementsBefore(CollectionUtil.Reverse(preconditionsOrPostconditions), 
                    _currentStatement.Statement);

            return textControl => textControl.Caret.MoveTo(updatedCurrentStatement ?? _currentStatement.Statement);
        }

        public override string FixName
        {
            get { return "Move contract call(s) before EndContractBlock"; }
        }

        protected override bool IsFixable(ValidationResult validationResult)
        {
            return IsFixableCore(validationResult);
        }
    }

    internal sealed class RemoveRedundantStatementFix : MalformedContractFix
    {
        public RemoveRedundantStatementFix(ValidationResult currentStatement, ValidatedContractBlock validatedContractBlock)
            : base(currentStatement, validatedContractBlock)
        { }

        public static bool IsFixableCore(ValidationResult validationResult)
        {
            return validationResult.Match(
                _ => false,
                error => error.Error == MalformedContractError.DuplicatedEndContractBlock ||
                         error.Error == MalformedContractError.EnsuresInVoidReturnMethod,
                _ => false);
        }

        protected override Action<ITextControl> DoExecuteFix(IList<ValidationResult> statementsToFix)
        {
            _currentStatement.Statement.RemoveOrReplaceByEmptyStatement();
            return null;
        }

        public override string FixName
        {
            get
            {
                return
                    _currentStatement.Match(
                        _ => string.Empty,
                        error => GetFixName(error.Error),
                        warning => string.Empty);
            }
        }

        protected override bool IsFixable(ValidationResult validationResult)
        {
            return IsFixableCore(validationResult);
        }

        private static string GetFixName(MalformedContractError error)
        {
            if (error == MalformedContractError.DuplicatedEndContractBlock)
                return "Remove redundant EndContractBlock";

            if (error == MalformedContractError.ContractStatementInTheMiddleOfTheMethod)
                return "Remove redundant Contract.Ensures";

            return "Remove redundant contact statement";
        }
    }

    internal sealed class FixContractResultTypeInContractEnsure : MalformedContractFix
    {
        private readonly IType _targetType;
        private readonly string _fixName;
        public FixContractResultTypeInContractEnsure(ValidationResult currentStatement,
            ValidatedContractBlock validatedContractBlock)
            : base(currentStatement, validatedContractBlock)
        {
            Contract.Assert(currentStatement.ProcessedStatement.CodeContractStatement != null);
            
            var declaredMethod = currentStatement.ProcessedStatement.CodeContractStatement.GetDeclaredMethod();
            _targetType = declaredMethod
                .With(x => x.DeclaredElement)
                .Return(x => x.ReturnType);

            _fixName = string.Format("Change to 'Contract.Result<{0}>()",
                _targetType.GetPresentableName(CSharpLanguage.Instance));
        }

        public static bool IsFixableCore(ValidationResult validationResult)
        {
            return validationResult.Match(
                _ => false,
                error => error.Error == MalformedContractError.ResultTypeInEnsuresIsIncompatibleWithMethodReturnType,
                _ => false);
        }

        protected override Action<ITextControl> DoExecuteFix(IList<ValidationResult> toFix)
        {
            Contract.Assert(_currentStatement.ProcessedStatement.CodeContractStatement != null);

            var ensures =
                _currentStatement.ProcessedStatement.CodeContractStatement.CodeContractExpression.Value as ContractEnsures;
            
            Contract.Assert(ensures != null);
            ensures.SetContractResultType(_targetType);
            
            return null;
        }

        public override string FixName
        {
            get { return _fixName; }
        }

        protected override bool IsFixable(ValidationResult validationResult)
        {
            return validationResult
                .Match(_ => false,
                    error => error.Error == MalformedContractError.ResultTypeInEnsuresIsIncompatibleWithMethodReturnType,
                    warning => false);
        }
    }

    //internal sealed class MoveContractOutOfTheTryBlock : MalformedContractFix
    //{
    //    public MoveContractOutOfTheTryBlock(ValidationResult currentStatement, ValidatedContractBlock validatedContractBlock)
    //        : base(currentStatement, validatedContractBlock)
    //    { }

    //    public static bool IsFixableCore(ValidationResult validationResult)
    //    {
    //        return validationResult.Match(
    //            _ => false,
    //            error => error.Error == MalformedContractError.MethodContractInTryBlock,
    //            _ => false);
    //    }

    //    private static bool StatementInsideTryBlock(ICSharpStatement statement)
    //    {
    //        Contract.Requires(statement != null);
    //        return statement.GetContainingNode<ITryStatement>() != null;
    //    }

    //    private ICSharpStatement GetLastValidContractStatement()
    //    {
    //        // Looking for the first contract statement that not in the try block
    //        foreach (var validatedStatement in _validatedContractBlock.ValidatedBlock.Reverse())
    //        {
    //            if (validatedStatement.ProcessedStatement.ContractStatement != null &&
    //                !StatementInsideTryBlock(validatedStatement.ProcessedStatement.CSharpStatement))
    //            {
    //                return validatedStatement.ProcessedStatement.CSharpStatement;
    //            }
    //        }

    //        return null;
    //    }

    //    protected override Action<ITextControl> DoExecuteFix(IList<ValidationResult> statementsToFix)
    //    {
    //        // Looking for the block where all the statements should be.
    //        // This method should be called before current statement would be detached from the parent!
    //        var lastValidContractStatement = GetLastValidContractStatement();
    //        IBlock targetBlock = null;

    //        if (lastValidContractStatement == null)
    //        { 
    //            targetBlock = ContractFixUtils.GetTargetBlock(_currentStatement.Statement);
    //            Contract.Assert(targetBlock != null, "Target block should exist!");
    //        }

    //        var statements = statementsToFix.Select(x => x.Statement).ToList();

    //        // Removing all illegal statements
    //        foreach (var s in statements)
    //        {
    //            s.RemoveOrReplaceByEmptyStatement();
    //        }

    //        ICSharpStatement updatedCurrentStatement = null;

    //        if (lastValidContractStatement != null)
    //        {
    //            // And adding them to the last contract in valid block
    //            updatedCurrentStatement =
    //                lastValidContractStatement.AddStatementsAfter(statements, _currentStatement.Statement);
    //        }
    //        else
    //        {
    //            // or to the special block!
    //            updatedCurrentStatement = targetBlock.AddStatementsTo(statements, _currentStatement.Statement);
    //        }

    //        return textControl => textControl.Caret.MoveTo(updatedCurrentStatement ?? _currentStatement.Statement);
    //    }

    //    public override string FixName
    //    {
    //        get { return "Move contract call(s) out of the try block"; }
    //    }

    //    protected override bool IsFixable(ValidationResult validationResult)
    //    {
    //        return IsFixableCore(validationResult);
    //    }
    //}
}