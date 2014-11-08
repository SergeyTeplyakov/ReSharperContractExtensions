using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows.Controls;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Represents simple validation strategy for the <see cref="ProcessedStatement"/>.
    /// </summary>
    public abstract class ValidationRule
    {
        protected ValidationRule()
        {}

        protected abstract ValidationResult DoValidate(ProcessedStatement currentStatement,
            IList<ProcessedStatement> contractBlock);

        public ValidationResult Validate(ProcessedStatement currentStatement, IList<ProcessedStatement> contractBlock)
        {
            var validationResult = DoValidate(currentStatement, contractBlock);
            validationResult.SetProcessedStatement(currentStatement);
            return validationResult;
        }

        /// <summary>
        /// Factory method for validating non-processed contract statement.
        /// </summary>
        public static ValidationRule CheckStatement(Func<ICSharpStatement, ValidationResult> validationFunc)
        {
            Contract.Requires(validationFunc != null);
            return new StatementValidationRule(validationFunc);
        }

        /// <summary>
        /// Factory method for validating processed <see cref="CodeContractStatement"/> from the <see cref="ContractBlock"/>.
        /// </summary>
        public static ValidationRule CheckCodeContractStatement(Func<CodeContractStatement, ValidationResult> validationFunc)
        {
            Contract.Requires(validationFunc != null);
            return new CodeContractStatementValidationRule(validationFunc);
        }

        public static ValidationRule CheckContractStatement(Func<ContractStatement, ValidationResult> validationFunc)
        {
            Contract.Requires(validationFunc != null);
            return new ContractStatementValidationRule(validationFunc);
        }

        /// <summary>
        /// Factory method for validating full processed contract block.
        /// </summary>
        public static ValidationRule CheckContractBlock(
            Func<CodeContractStatement, IList<ProcessedStatement>, ValidationResult> validationFunc)
        {
            Contract.Requires(validationFunc != null);
            return new CodeContractStatementWithContractBlockValidationRule(validationFunc);
        }
    }

    public class ContractStatementValidationRule : ValidationRule
    {
        private readonly Func<ContractStatement, ValidationResult> _validationFunc;

        public ContractStatementValidationRule(Func<ContractStatement, ValidationResult> validationFunc)
        {
            Contract.Requires(validationFunc != null);

            _validationFunc = validationFunc;
        }

        protected override ValidationResult DoValidate(ProcessedStatement currentStatement, IList<ProcessedStatement> contractBlock)
        {
            if (currentStatement.ContractStatement == null)
                return ValidationResult.CreateNoError(currentStatement.CSharpStatement);
            return _validationFunc(currentStatement.ContractStatement);
        }
    }

    internal sealed class StatementValidationRule : ValidationRule
    {
        private readonly Func<ICSharpStatement, ValidationResult> _statementValidationRule;
        public StatementValidationRule(Func<ICSharpStatement, ValidationResult> statementValidationRule)
        {
            Contract.Requires(statementValidationRule != null);

            _statementValidationRule = statementValidationRule;
        }

        protected override ValidationResult DoValidate(ProcessedStatement currentStatement, 
            IList<ProcessedStatement> contractBlock)
        {
            if (currentStatement.ContractStatement != null)
                return ValidationResult.CreateNoError(currentStatement.CSharpStatement);

            return _statementValidationRule(currentStatement.CSharpStatement);
        }
    }

    internal sealed class CodeContractStatementValidationRule : ValidationRule
    {
        private readonly Func<CodeContractStatement, ValidationResult> _contractValidationRule;

        public CodeContractStatementValidationRule(Func<CodeContractStatement, ValidationResult> contractValidationRule)
        {
            Contract.Requires(contractValidationRule != null);
            _contractValidationRule = contractValidationRule;
        }

        protected override ValidationResult DoValidate(ProcessedStatement currentStatement, IList<ProcessedStatement> contractBlock)
        {
            if (currentStatement.CodeContractStatement == null)
                return ValidationResult.CreateNoError(currentStatement.CSharpStatement);

            return _contractValidationRule(currentStatement.CodeContractStatement);
        }
    }

    internal sealed class CodeContractStatementWithContractBlockValidationRule : ValidationRule
    {
        private readonly Func<CodeContractStatement, IList<ProcessedStatement>, ValidationResult> _contractBlockValidationRule;

        public CodeContractStatementWithContractBlockValidationRule(
            Func<CodeContractStatement, IList<ProcessedStatement>, ValidationResult> contractBlockValidationRule)
        {
            Contract.Requires(contractBlockValidationRule != null);
            _contractBlockValidationRule = contractBlockValidationRule;
        }

        protected override ValidationResult DoValidate(ProcessedStatement currentStatement, IList<ProcessedStatement> contractBlock)
        {
            if (currentStatement.CodeContractStatement == null)
                return ValidationResult.CreateNoError(currentStatement.CSharpStatement);

            return _contractBlockValidationRule(currentStatement.CodeContractStatement, contractBlock);
        }
    }
}