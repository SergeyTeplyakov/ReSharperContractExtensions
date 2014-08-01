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
        // This is some kind of discriminated union with only one possible rule
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
            // Current rule checks non-processed statement
            if (_statementValidationRule != null)
            {
                if (currentStatement.ContractStatement != null)
                    return ValidationResult.CreateNoError(currentStatement.CSharpStatement);
                return _statementValidationRule(currentStatement.CSharpStatement);
            }

            // Other rules are applicable only for contract statements.
            // For now, there are now other rules for any other specific contract statemetns
            // except CodeContractStatements

            if (currentStatement.CodeContractStatement == null)
                return ValidationResult.CreateNoError(currentStatement.CSharpStatement);

            if (_contractValidationRule != null)
            {
                return _contractValidationRule(currentStatement.CodeContractStatement);
            }

            Contract.Assert(_contractBlockValidationRule != null);
            return _contractBlockValidationRule(contractBlock, currentStatement);
        }

        /// <summary>
        /// Factory method for validating non-processed contract statement.
        /// </summary>
        public static ValidationRule CheckStatement(Func<ICSharpStatement, ValidationResult> validationFunc)
        {
            Contract.Requires(validationFunc != null);
            return new ValidationRule(validationFunc);
        }

        /// <summary>
        /// Factory method for validating processed <see cref="CodeContractStatement"/> from the <see cref="ContractBlock"/>.
        /// </summary>
        public static ValidationRule CheckContractStatement(Func<CodeContractStatement, ValidationResult> validationFunc)
        {
            Contract.Requires(validationFunc != null);
            return new ValidationRule(validationFunc);
        }

        /// <summary>
        /// Factory method for validating full processed contract block.
        /// </summary>
        public static ValidationRule CheckContractBlock(Func<IList<ProcessedStatement>, ProcessedStatement, ValidationResult> validationFunc)
        {
            Contract.Requires(validationFunc != null);
            return new ValidationRule(validationFunc);
        }
    }
}