using System;
using System.Diagnostics.Contracts;
using ReSharper.ContractExtensions.ContractsEx.Assertions.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Proxy class taht contains single validation rule as a func.
    /// </summary>
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