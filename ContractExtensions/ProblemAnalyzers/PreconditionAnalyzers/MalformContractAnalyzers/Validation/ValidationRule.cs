using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Represents validation rule for <see cref="ProcessedStatement"/>.
    /// </summary>
    internal sealed class ValidationRule
    {
        private readonly Func<IList<ProcessedStatement>, ProcessedStatement, ValidationResult> _contractBlockValidationRule;
        private readonly Func<CodeContractStatement, ValidationResult> _contractValidationRule;
        private readonly Func<ICSharpStatement, ValidationResult> _statementValidationRule;

        private ValidationRule(Func<ICSharpStatement, ValidationResult> statementValidationRule)
        {
            Contract.Requires(statementValidationRule != null);
            _statementValidationRule = statementValidationRule;
        }

        private ValidationRule(Func<CodeContractStatement, ValidationResult> contractValidationRule)
        {
            Contract.Requires(contractValidationRule != null);

            _contractValidationRule = contractValidationRule;
        }

        private ValidationRule(Func<IList<ProcessedStatement>, ProcessedStatement, ValidationResult> contractBlockValidationRule)
        {
            Contract.Requires(contractBlockValidationRule != null);
            _contractBlockValidationRule = contractBlockValidationRule;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_statementValidationRule != null || 
                               _contractValidationRule != null ||
                               _contractBlockValidationRule != null,
                "At least one validator should exists");
        }

        public ValidationResult Validate(ProcessedStatement currentStatement, IList<ProcessedStatement> contractBlock)
        {
            if (_statementValidationRule != null)
            {
                if (currentStatement.ContractStatement != null)
                    return ValidationResult.CreateNoError(currentStatement.CSharpStatement);
                return _statementValidationRule(currentStatement.CSharpStatement);
            }

            if (currentStatement.ContractStatement == null)
                return ValidationResult.CreateNoError(currentStatement.CSharpStatement);

            if (_contractValidationRule != null)
            {

                return _contractValidationRule(currentStatement.ContractStatement);
            }

            Contract.Assert(_contractBlockValidationRule != null);
            return _contractBlockValidationRule(contractBlock, currentStatement);
        }

        public static ValidationRule CheckStatement(Func<ICSharpStatement, ValidationResult> validationFunc)
        {
            return new ValidationRule(validationFunc);
        }

        public static ValidationRule CheckContractStatement(Func<CodeContractStatement, ValidationResult> validationFunc)
        {
            return new ValidationRule(validationFunc);
        }

        public static ValidationRule CheckContractBlock(Func<IList<ProcessedStatement>, ProcessedStatement, ValidationResult> validationFunc)
        {
            return new ValidationRule(validationFunc);
        }
    }
}