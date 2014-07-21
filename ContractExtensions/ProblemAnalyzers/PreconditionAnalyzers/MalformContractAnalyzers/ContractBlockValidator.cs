using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Statements;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal enum ErrorType
    {
        CodeContractError,
        CodeContractWarning,
        CustomWarning,
        NoError
    }

    /// <summary>
    /// Represents result of validation process for a statements in the conract block.
    /// </summary>
    /// <remarks>
    /// This type is actually a discriminated union, with a set of potential combinations, like
    /// if ErrorType i CodeContractError then MalformedContractError property should be used.
    /// But for now I don't want more complicated implementations!
    /// </remarks>
    internal sealed class ValidationResult
    {
        public ErrorType ErrorType { get; private set; }
        public ICSharpStatement Statement { get; private set; }

        public MalformedContractError MalformedContractError { get; private set; }
        public MalformedContractWarning MalformedContractWarning { get; private set; }

        public static ValidationResult NoError
        {
            get { return new ValidationResult { ErrorType = ErrorType.NoError }; }
        }

        public static ValidationResult CreatNoError(ICSharpStatement statement)
        {
            return new ValidationResult { Statement = statement, ErrorType = ErrorType.NoError };
        }

        public static ValidationResult CreateError(ICSharpStatement statement, MalformedContractError error)
        {
            return new ValidationResult { Statement = statement, ErrorType = ErrorType.CodeContractError, MalformedContractError = error };
        }

        public static ValidationResult CreateWarning(ICSharpStatement statement, MalformedContractWarning warning)
        {
            return new ValidationResult { Statement = statement, ErrorType = ErrorType.CodeContractWarning, MalformedContractWarning = warning };
        }
    }

    internal class ValidationRule
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

        public ValidationResult Run(IList<ProcessedStatement> contractBlock, ProcessedStatement currentStatement)
        {
            if (_statementValidationRule != null)
            {
                if (currentStatement.ContractStatement != null)
                    return ValidationResult.NoError;
                return _statementValidationRule(currentStatement.CSharpStatement);
            }

            if (_contractValidationRule != null)
            {
                if (currentStatement.ContractStatement == null)
                    return ValidationResult.NoError;

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

    internal static class ContractBlockValidator
    {
        private static readonly List<ValidationRule> _validationRules = GetValidationRules().ToList();

        public static IEnumerable<ValidationResult> ValidateContractBlockStatements(
            IList<ProcessedStatement> contractBlock)
        {
            var query =
                from currentStatement in contractBlock
                let validationResult = ValidateStatement(contractBlock, currentStatement)
                select validationResult;

            return query.ToList();
        }

        private static IEnumerable<ValidationRule> GetValidationRules()
        {
            yield return ValidationRule.CheckStatement(
                s =>
                {
                    // Void-return method are forbidden in contract block
                    if (!IsMarkedWithContractValidationAttributeMethod(s) && IsVoidReturnMethod(s))
                        return ValidationResult.CreateError(s, MalformedContractError.VoidReturnMethodCall);
                    return ValidationResult.NoError;
                });

            yield return ValidationRule.CheckStatement(
                s =>
                {
                    // Non-void return methods lead to warning from the compiler
                    if (!IsMarkedWithContractValidationAttributeMethod(s) && IsNonVoidReturnMethod(s))
                        return ValidationResult.CreateWarning(s, MalformedContractWarning.NonVoidReturnMethodCall);
                    return ValidationResult.NoError;
                });

            yield return ValidationRule.CheckStatement(
                s =>
                {
                    // Assignments are forbidden in contract block
                    if (IsAssignmentStatement(s))
                        return ValidationResult.CreateError(s, MalformedContractError.AssignmentInContractBlock);
                    return ValidationResult.NoError;
                });

            yield return ValidationRule.CheckContractStatement(
                s =>
                {
                    // Assert/Assume are forbidden in contract block
                    if ((s.StatementType == CodeContractStatementType.Assert ||
                        s.StatementType == CodeContractStatementType.Assume))
                        return ValidationResult.CreateError(s.Statement, MalformedContractError.AssertOrAssumeInContractBlock);
                    return ValidationResult.NoError;
                });

            yield return ValidationRule.CheckContractBlock(
                (contractBlock, currentStatement) =>
                {
                    // Ensures/Ensures on throw should not be before requires
                    if (currentStatement.ContractStatement.IsPostcondition && HasPreconditionAfterCurrentStatement(contractBlock, currentStatement))
                        return ValidationResult.CreateError(currentStatement.CSharpStatement,
                            MalformedContractError.RequiresAfterEnsures);
                    return ValidationResult.NoError;
                });

            yield return ValidationRule.CheckContractBlock(
                (contractBlock, currentStatement) =>
                {
                    // Ensures/Ensures on throw should not be before requires
                    if (currentStatement.ContractStatement.IsPrecondition && HasPostconditionsBeforeCurentStatement(contractBlock, currentStatement))
                        return ValidationResult.CreateError(currentStatement.CSharpStatement,
                            MalformedContractError.RequiresAfterEnsures);
                    return ValidationResult.NoError;
                });
        }

        private static bool HasPreconditionAfterCurrentStatement(IList<ProcessedStatement> contractBlock, ProcessedStatement currentStatement)
        {
            var index = contractBlock.IndexOf(currentStatement);
            Contract.Assert(index != -1, "Current statement should be inside contract block");

            return
                contractBlock.Skip(index + 1)
                    .Any(cs => cs.ContractStatement != null && cs.ContractStatement.IsPrecondition);
        }

        private static bool HasPostconditionsBeforeCurentStatement(IList<ProcessedStatement> contractBlock, ProcessedStatement currentStatement)
        {
            var index = contractBlock.IndexOf(currentStatement);
            Contract.Assert(index != -1, "Current statement should be inside contract block");

            return
                contractBlock.Take(index)
                    .Any(cs => cs.ContractStatement != null && cs.ContractStatement.IsPostcondition);
        }

        private static ValidationResult ValidateStatement(IList<ProcessedStatement> contractBlock, ProcessedStatement currentStatement)
        {
            Contract.Requires(currentStatement != null);
            Contract.Ensures(Contract.Result<ValidationResult>() != null);

            return _validationRules
                .Select(rule => rule.Run(contractBlock, currentStatement))
                .FirstOrDefault(vr => vr.ErrorType != ErrorType.NoError)
                    ?? ValidationResult.CreatNoError(currentStatement.CSharpStatement);
        }

        private static bool IsAssignmentStatement(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            return statement.With(x => x as IExpressionStatement)
                    .With(x => x.Expression as IAssignmentExpression) != null;
        }


        private static bool IsMarkedWithContractValidationAttributeMethod(ICSharpStatement statement)
        {
            var validatorAttribute = new ClrTypeName("System.Diagnostics.Contracts.ContractArgumentValidatorAttribute");
            return GetInvokedMethod(statement)
                .ReturnStruct(x => x.HasAttributeInstance(validatorAttribute, false)) == true;
        }

        private static bool IsVoidReturnMethod(ICSharpStatement statement)
        {
            return
                GetInvokedMethod(statement)
                    .ReturnStruct(x => x.ReturnType.IsVoid()) == true;
        }

        private static bool IsNonVoidReturnMethod(ICSharpStatement statement)
        {
            return
                GetInvokedMethod(statement)
                    .ReturnStruct(x => !x.ReturnType.IsVoid()) == false;
        }

        [CanBeNull]
        private static IMethod GetInvokedMethod(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            return statement
                .With(x => x as IExpressionStatement)
                .With(x => x.Expression as IInvocationExpression)
                .With(x => x.InvocationExpressionReference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement as IMethod);
        }
    }
}