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

    internal static class ContractBlockValidator
    {
        private static readonly 
            List<Func<ProcessedStatement, ValidationResult>> _malformStatementDetectors = GetMalformStatementDetectors().ToList();

        public static IEnumerable<ValidationResult> ValidateContractBlockStatements(
            IEnumerable<ProcessedStatement> contractBlockStatements)
        {
            var query =
                from statement in contractBlockStatements
                let validationResult = ValidateStatement(statement)
                select validationResult;

            return query.ToList();
        }


        private static IEnumerable<Func<ProcessedStatement, ValidationResult>> GetMalformStatementDetectors()
        {
            yield return
                s =>
                {
                    // Or boy, I NEED Discriminated union here!!!

                    // Void-return method are forbidden in contract block
                    if (s.ContractStatement == null &&
                        !IsMarkedWithContractValidationAttributeMethod(s.CSharpStatement) &&
                        IsVoidReturnMethod(s.CSharpStatement))
                        return ValidationResult.CreateError(s.CSharpStatement, MalformedContractError.VoidReturnMethodCall);
                    return ValidationResult.NoError;
                };

            yield return
                s =>
                {
                    // Non-void return methods lead to warning from the compiler
                    if (s.ContractStatement == null &&
                        !IsMarkedWithContractValidationAttributeMethod(s.CSharpStatement) &&
                        IsNonVoidReturnMethod(s.CSharpStatement))
                        return ValidationResult.CreateWarning(s.CSharpStatement, MalformedContractWarning.NonVoidReturnMethodCall);
                    return ValidationResult.NoError;
                };

            yield return
                s =>
                {
                    // Assignments are forbidden in contract block
                    if (s.ContractStatement == null && IsAssignmentStatement(s.CSharpStatement))
                        return ValidationResult.CreateError(s.CSharpStatement,
                            MalformedContractError.AssignmentInContractBlock);
                    return ValidationResult.NoError;
                };


            yield return
                s =>
                {
                    // Assert/Assume are forbidden in contract block
                    if (s.ContractStatement != null &&
                        (s.ContractStatement.StatementType == CodeContractStatementType.Assert ||
                        s.ContractStatement.StatementType == CodeContractStatementType.Assume))
                        return ValidationResult.CreateError(s.CSharpStatement, MalformedContractError.AssertOrAssumeInContractBlock);
                    return ValidationResult.NoError;
                };
        }

        private static ValidationResult ValidateStatement(ProcessedStatement statement)
        {
            Contract.Requires(statement != null);
            Contract.Ensures(Contract.Result<ValidationResult>() != null);

            return _malformStatementDetectors
                .Select(detector => detector(statement))
                .FirstOrDefault(vr => vr.ErrorType != ErrorType.NoError)
                    ?? ValidationResult.CreatNoError(statement.CSharpStatement);
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