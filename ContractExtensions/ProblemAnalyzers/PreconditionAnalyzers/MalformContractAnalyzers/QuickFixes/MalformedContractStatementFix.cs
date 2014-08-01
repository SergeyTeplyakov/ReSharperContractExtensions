using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using ReSharper.ContractExtensions.ContractsEx.Statements;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal interface IMalformedContractStatementFix
    {
        bool IsApplicable(ValidationResult validationResult, CodeContractStatement contractStatement);
        Action<ITextControl> Apply(ValidationResult validationResult, CodeContractStatement contractStatement);
        string GetText();
    }

    internal abstract class MalformedContractStatementFix : IMalformedContractStatementFix
    {
        private static List<IMalformedContractStatementFix> _fixes = CreateFixes().ToList();

        private static IEnumerable<IMalformedContractStatementFix> CreateFixes()
        {
            yield return new RemoveRedundantContractStatementFix();
            yield return new MoveStatementToTheContractBlock();
            yield return new ConvertPreconditionToAssertFix();
        }

        public static IMalformedContractStatementFix TryCreate(ValidationResult validationResult,
            CodeContractStatement contractStatement)
        {
            Contract.Requires(validationResult != null);
            Contract.Requires(contractStatement != null);

            return _fixes.FirstOrDefault(f => f.IsApplicable(validationResult, contractStatement));
        }


        public abstract bool IsApplicable(ValidationResult validationResult, CodeContractStatement contractStatement);
        public abstract Action<ITextControl> Apply(ValidationResult validationResult, CodeContractStatement contractStatement);
        public abstract string GetText();
    }

    internal sealed class RemoveRedundantContractStatementFix : MalformedContractStatementFix
    {
        public override bool IsApplicable(ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            return validationResult.Match(_ => false,
                error => error.Error == MalformedContractError.ContractStatementInTheMiddleOfTheMethod &&
                         contractStatement.IsEndContractBlock,
                _ => false);
        }

        public override Action<ITextControl> Apply(
            ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            contractStatement.Statement.RemoveOrReplaceByEmptyStatement();
            return null;
        }

        public override string GetText()
        {
            return "Remove redundant call to Contract.EndContractBlock()";
        }
    }

    internal sealed class MoveStatementToTheContractBlock : MalformedContractStatementFix
    {
        public override bool IsApplicable(ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            return validationResult.Match(_ => false,
                error => 
                    (error.Error == MalformedContractError.ContractStatementInTheMiddleOfTheMethod &&
                         contractStatement.IsPostcondition) ||
                    (error.Error == MalformedContractError.MethodContractInTryBlock),
                _ => false);
        }

        public override Action<ITextControl> Apply(
            ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            // This fix contains following steps:
            // 1. Removing original postcondition
            // 2. Looking for contract block of the current method
            // 3. If this block exists, fix should move postcondition to the appropriate place 
            //    (after last postcondition or precondition)
            // 4. Otherwise fix should move the precondition at the beginning of the method.

            // We should get contract block and potential target block before
            // removing current statement
            var contractBlock = GetContractBlock(contractStatement);

            var containingFunction = GetContainingFunction(contractStatement.Statement);

            contractStatement.Statement.RemoveOrReplaceByEmptyStatement();

            ICSharpStatement anchor = GetAnchor(contractBlock, contractStatement);

            ICSharpStatement updatedStatement;

            if (anchor != null)
            {
                updatedStatement = anchor.AddStatementsAfter(new[] {contractStatement.Statement}, contractStatement.Statement);
            }
            else
            {
                updatedStatement = containingFunction.Body.AddStatementAfter(contractStatement.Statement, null);
            }

            return textControl => textControl.Caret.MoveTo(updatedStatement);
        }

        public override string GetText()
        {
            return "Move statement to the method contract block";
        }

        private static ICSharpFunctionDeclaration GetContainingFunction(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            Contract.Ensures(Contract.Result<ICSharpFunctionDeclaration>() != null);

            return (ICSharpFunctionDeclaration) statement.GetContainingTypeMemberDeclaration();
        }

        private IList<ProcessedStatement> GetContractBlock(CodeContractStatement contractStatement)
        {
            var method = contractStatement.Statement.GetContainingNode<ICSharpFunctionDeclaration>();
            Contract.Assert(method != null);

            return method.GetContractBlockStatements();
        }

        [CanBeNull]
        private static ICSharpStatement GetAnchor(IList<ProcessedStatement> statements, 
            CodeContractStatement contractStatement)
        {
            // Looking for the last precondition if we're moving precondition
            // or looking for the last postcondition or precondition for Ensures and EndContractBlock
            return statements
                .Where(s => s.CodeContractStatement != null)
                .Reverse()
                .FirstOrDefault(
                    s =>
                    {
                        if (contractStatement.IsPrecondition)
                            return s.CodeContractStatement.IsPostcondition;
                        return s.CodeContractStatement.IsPostcondition || s.CodeContractStatement.IsPrecondition;
                    })
                .Return(x => x.CSharpStatement);
        }

    }

    internal sealed class ConvertPreconditionToAssertFix : MalformedContractStatementFix
    {
        public override bool IsApplicable(ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            return validationResult.Match(_ => false,
                error => error.Error == MalformedContractError.ContractStatementInTheMiddleOfTheMethod &&
                         contractStatement.IsPrecondition,
                _ => false);
        }

        public override Action<ITextControl> Apply(ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            var assertStatement = CreateAssertStatement(contractStatement);
            contractStatement.Statement.ReplaceBy(assertStatement);

            return null;
        }

        public override string GetText()
        {
            return "Convert precondition statement to Contract.Assert()";
        }

        private ICSharpStatement CreateAssertStatement(CodeContractStatement contractStatement)
        {
            var invocationExpression = contractStatement.InvocationExpression;
            Contract.Assert(invocationExpression.Arguments.Count != 0);

            var factory = CSharpElementFactory.GetInstance(contractStatement.Statement);

            if (invocationExpression.Arguments.Count == 1)
            {
                return factory.CreateStatement("Contract.Assert($0);", invocationExpression.Arguments[0]);
            }
            return factory.CreateStatement("Contract.Assert($0, $1);", invocationExpression.Arguments[0], invocationExpression.Arguments[1]);
        }

    }
}