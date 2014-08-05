using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal sealed class SingleStatementValidationRule
    {
        private readonly Func<CodeContractStatement, ValidationResult> _statementValidationRule;

        private SingleStatementValidationRule(Func<CodeContractStatement, ValidationResult> statementValidationRule)
        {
            Contract.Requires(statementValidationRule != null);
            _statementValidationRule = statementValidationRule;
        }

        public ValidationResult Validate(CodeContractStatement statement)
        {
            Contract.Requires(statement != null);
            Contract.Ensures(Contract.Result<ValidationResult>() != null);

            return _statementValidationRule(statement);
        }

        public static SingleStatementValidationRule Create(Func<CodeContractStatement, ValidationResult> statementValidationRule)
        {
            Contract.Requires(statementValidationRule != null);
            Contract.Ensures(Contract.Result<SingleStatementValidationRule>() != null);

            return new SingleStatementValidationRule(statementValidationRule);
        }
    }
}