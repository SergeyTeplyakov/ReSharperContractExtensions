using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal interface IMalformedContractStatementFix
    {
        bool IsApplicable(ValidationResult validationResult, CodeContractStatement contractStatement);
        void Apply(ValidationResult validationResult, CodeContractStatement contractStatement);
        string GetText();
    }

    internal abstract class MalformedContractStatementFix : IMalformedContractStatementFix
    {
        private static List<IMalformedContractStatementFix> _fixes = CreateFixes().ToList();

        private static IEnumerable<IMalformedContractStatementFix> CreateFixes()
        {
            yield return new RemoveRedundantContractStatementFix();
            yield return new MoveStatementAtTheEndOfContractBlockFix();
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
        public abstract void Apply(ValidationResult validationResult, CodeContractStatement contractStatement);
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

        public override void Apply(ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            contractStatement.Statement.RemoveOrReplaceByEmptyStatement();
        }

        public override string GetText()
        {
            return "Remove redundant call to Contract.EndContractBlock()";
        }
    }

    internal sealed class MoveStatementAtTheEndOfContractBlockFix : MalformedContractStatementFix
    {
        public override bool IsApplicable(ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            return validationResult.Match(_ => false,
                error => error.Error == MalformedContractError.ContractStatementInTheMiddleOfTheMethod &&
                         contractStatement.IsPostcondition,
                _ => false);
        }

        public override void Apply(ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            throw new NotImplementedException();
        }

        public override string GetText()
        {
            return "Move postcondition to the method contract block";
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

        public override void Apply(ValidationResult validationResult, CodeContractStatement contractStatement)
        {
            var assertStatement = CreateAssertStatement(contractStatement);
            contractStatement.Statement.ReplaceBy(assertStatement);
        }

        public override string GetText()
        {
            return "Convert precondition statement to Contract.Assert()";
        }

        private ICSharpStatement CreateAssertStatement(CodeContractStatement contractStatement)
        {
            var invocationExpression = contractStatement.InvocationExpression;
            Contract.Assert(invocationExpression != null);
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