using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Settings.Storage.Persistence;
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
    [ContractClass(typeof (ValidationResultContract))]
    internal abstract class ValidationResult
    {
        private readonly ICSharpStatement _statement;
        protected ValidationResult(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            _statement = statement;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Statement != null);
        }

        public T Match<T>(
            Func<NoErrorValidationResult, T> noErrorMatch,
            Func<CodeContractErrorValidationResult, T> errorMatch,
            Func<CodeContractWarningValidationResult, T> warningMatch)
        {
            Contract.Requires(noErrorMatch != null);
            Contract.Requires(errorMatch != null);
            Contract.Requires(warningMatch != null);

            var noErrorResult = this as NoErrorValidationResult;
            if (noErrorResult != null)
                return noErrorMatch(noErrorResult);

            var errorResult = this as CodeContractErrorValidationResult;
            if (errorResult != null)
                return errorMatch(errorResult);

            var warningResult = this as CodeContractWarningValidationResult;
            if (warningResult != null)
                return warningMatch(warningResult);

            Contract.Assert(false, "Unknown validation result type: " + GetType());
            throw new InvalidOperationException("Unknown validation result type: " + GetType());
        }

        public abstract ErrorType ErrorType { get; }

        public string GetErrorText()
        {
            return DoGetErrorText(GetEnclosingMethodName());
        }

        protected abstract string DoGetErrorText(string methodName);

        public ICSharpStatement Statement { get { return _statement; } }

        private string GetEnclosingMethodName()
        {
            return Statement.GetContainingTypeMemberDeclaration().DeclaredName;
        }

        public static ValidationResult CreateNoError(ICSharpStatement statement)
        {
            return new NoErrorValidationResult(statement);
        }

        public static ValidationResult CreateError(ICSharpStatement statement, MalformedContractError error)
        {
            return new CodeContractErrorValidationResult(statement, error);
        }

        public static ValidationResult CreateWarning(ICSharpStatement statement, MalformedContractWarning warning)
        {
            return new CodeContractWarningValidationResult(statement, warning);
        }
    }

    [ContractClassFor(typeof (ValidationResult))]
    abstract class ValidationResultContract : ValidationResult
    {
        protected ValidationResultContract(ICSharpStatement statement) : base(statement)
        {}

        protected override string DoGetErrorText(string methodName)
        {
            Contract.Requires(!string.IsNullOrEmpty(methodName));
            Contract.Ensures(Contract.Result<string>() != null);

            throw new NotImplementedException();
        }
    }

    internal sealed class NoErrorValidationResult : ValidationResult
    {
        public NoErrorValidationResult(ICSharpStatement statement) : base(statement)
        {}

        public override ErrorType ErrorType { get { return ErrorType.NoError; } }

        protected override string DoGetErrorText(string methodName)
        {
            return string.Empty;
        }
    }

    internal sealed class CodeContractErrorValidationResult : ValidationResult
    {
        public CodeContractErrorValidationResult(ICSharpStatement statement, MalformedContractError error) : base(statement)
        {
            Error = error;
        }

        public MalformedContractError Error { get; private set; }

        public override ErrorType ErrorType { get { return ErrorType.CodeContractError; } }

        protected override string DoGetErrorText(string methodName)
        {
            return Error.GetErrorText(methodName);
        }
    }

    internal sealed class CodeContractWarningValidationResult : ValidationResult
    {
        public CodeContractWarningValidationResult(ICSharpStatement statement, MalformedContractWarning warning) 
            : base(statement)
        {
            Warning = warning;
        }

        public override ErrorType ErrorType { get { return ErrorType.CodeContractWarning; } }

        public MalformedContractWarning Warning { get; private set; }

        protected override string DoGetErrorText(string methodName)
        {
            return Warning.GetErrorText(methodName);
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

    internal class ValidatedContractBlock
    {
        private readonly IList<ProcessedStatement> _contractBlock;
        private readonly IList<ValidationResult> _validationResults;

        public ValidatedContractBlock(IList<ProcessedStatement> contractBlock, IList<ValidationResult> validationResults)
        {
            Contract.Requires(contractBlock != null);
            Contract.Requires(contractBlock.Count != 0);
            Contract.Requires(contractBlock.Last().ContractStatement != null);
            Contract.Requires(validationResults != null);
            Contract.Requires(validationResults.Count == contractBlock.Count);

            _contractBlock = contractBlock;
            _validationResults = validationResults;
        }

        public ReadOnlyCollection<ProcessedStatement> ContractBlock
        {
            get { return new ReadOnlyCollection<ProcessedStatement>(_contractBlock); }
        }

        public ReadOnlyCollection<ValidationResult> ValidationResults
        {
            get { return new ReadOnlyCollection<ValidationResult>(_validationResults); }
        }
    }

    internal static class ContractBlockValidator
    {
        private static readonly List<ValidationRule> _validationRules = GetValidationRules().ToList();

        public static ValidatedContractBlock ValidateContractBlock(IList<ProcessedStatement> contractBlock)
        {
            return new ValidatedContractBlock(contractBlock, 
                ValidateContractBlockStatements(contractBlock).ToList());
        }

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
                    return ValidationResult.CreateNoError(s);
                });

            yield return ValidationRule.CheckStatement(
                s =>
                {
                    // Non-void return methods lead to warning from the compiler
                    if (!IsMarkedWithContractValidationAttributeMethod(s) && IsNonVoidReturnMethod(s))
                        return ValidationResult.CreateWarning(s, MalformedContractWarning.NonVoidReturnMethodCall);
                    return ValidationResult.CreateNoError(s);
                });

            yield return ValidationRule.CheckStatement(
                s =>
                {
                    // Assignments are forbidden in contract block
                    if (IsAssignmentStatement(s))
                        return ValidationResult.CreateError(s, MalformedContractError.AssignmentInContractBlock);
                    return ValidationResult.CreateNoError(s);
                });

            yield return ValidationRule.CheckContractStatement(
                s =>
                {
                    // Assert/Assume are forbidden in contract block
                    if ((s.StatementType == CodeContractStatementType.Assert ||
                        s.StatementType == CodeContractStatementType.Assume))
                        return ValidationResult.CreateError(s.Statement, MalformedContractError.AssertOrAssumeInContractBlock);
                    return ValidationResult.CreateNoError(s.Statement);
                });

            yield return ValidationRule.CheckContractBlock(
                (contractBlock, currentStatement) =>
                {
                    // Ensures/Ensures on throw should not be before requires
                    if (currentStatement.ContractStatement.IsPostcondition && HasPreconditionAfterCurrentStatement(contractBlock, currentStatement))
                        return ValidationResult.CreateError(currentStatement.CSharpStatement,
                            MalformedContractError.RequiresAfterEnsures);
                    return ValidationResult.CreateNoError(currentStatement.CSharpStatement);
                });

            yield return ValidationRule.CheckContractBlock(
                (contractBlock, currentStatement) =>
                {
                    // Ensures/Ensures on throw should not be before requires
                    if (currentStatement.ContractStatement.IsPrecondition && HasPostconditionsBeforeCurentStatement(contractBlock, currentStatement))
                        return ValidationResult.CreateError(currentStatement.CSharpStatement,
                            MalformedContractError.RequiresAfterEnsures);
                    return ValidationResult.CreateNoError(currentStatement.CSharpStatement);
                });

            yield return ValidationRule.CheckContractBlock(
                (contractBlock, currentStatement) =>
                {
                    // Ensures/Ensures on throw should not be before requires
                    if ((currentStatement.ContractStatement.IsPrecondition || 
                         currentStatement.ContractStatement.IsPostcondition) &&
                        HasEndContractBlockBeforeCurrentStatement(contractBlock, currentStatement))
                        return ValidationResult.CreateError(currentStatement.CSharpStatement,
                            MalformedContractError.ReqruiesOrEnsuresAfterEndContractBlock);
                    return ValidationResult.CreateNoError(currentStatement.CSharpStatement);
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
            foreach (var c in contractBlock)
            {
                if (c.Equals(currentStatement))
                    return false;

                if (c.ContractStatement != null && c.ContractStatement.IsPostcondition)
                    return true;
            }

            Contract.Assert(false, "Current statement not found in the contract block");
            throw new InvalidOperationException("Current statement not found in the contract block");
        }

        private static bool HasEndContractBlockBeforeCurrentStatement(IList<ProcessedStatement> contractBlock, ProcessedStatement currentStatement)
        {
            foreach (var c in contractBlock)
            {
                if (c.Equals(currentStatement))
                    return false;

                if (c.ContractStatement != null && 
                    c.ContractStatement.StatementType == CodeContractStatementType.EndContractBlock)
                    return true;
            }

            Contract.Assert(false, "Current statement not found in the contract block");
            throw new InvalidOperationException("Current statement not found in the contract block");
        }

        private static ValidationResult ValidateStatement(IList<ProcessedStatement> contractBlock, ProcessedStatement currentStatement)
        {
            Contract.Requires(currentStatement != null);
            Contract.Ensures(Contract.Result<ValidationResult>() != null);

            return _validationRules
                .Select(rule => rule.Run(contractBlock, currentStatement))
                .FirstOrDefault(vr => vr.ErrorType != ErrorType.NoError)
                    ?? ValidationResult.CreateNoError(currentStatement.CSharpStatement);
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
                    .ReturnStruct(x => x.ReturnType.IsVoid()) == false;
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