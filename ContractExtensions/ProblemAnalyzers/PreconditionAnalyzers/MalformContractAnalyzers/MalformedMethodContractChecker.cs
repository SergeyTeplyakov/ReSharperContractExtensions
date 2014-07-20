using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Statements;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal enum MalformedContractError
    {
        VoidReturnMethodCall,
        AssertOrAssumeInContractBlock,
        AssignmentInContractBlock,
    }
    /// <summary>
    /// Warns if method contract is malformed:
    /// - Contract statements are not the first statements in the method
    /// - Ensures statement is after precondition check
    /// </summary>
    [ElementProblemAnalyzer(new[] { typeof(ICSharpFunctionDeclaration) },
    HighlightingTypes = new[] { typeof(MalformedMethodContractHighlighting) })]
    public sealed class MalformedMethodContractChecker : ElementProblemAnalyzer<ICSharpFunctionDeclaration>
    {
        private enum ErrorType
        {
            Error,
            Warning,
            NoError
        }

        private class ValidationResult
        {
            public ErrorType ErrorType { get; private set; }
            public MalformedContractError MalformedContractError { get; private set; }
            public ICSharpStatement Statement { get; private set; }
            public string Message { get; private set; }

            public static ValidationResult NoError
            {
                get { return new ValidationResult {ErrorType = ErrorType.NoError}; }
            }

            public static ValidationResult CreatNoError(ICSharpStatement statement)
            {
                return new ValidationResult {Statement = statement, ErrorType = ErrorType.NoError};
            }

            public static ValidationResult CreateError(ICSharpStatement statement, MalformedContractError error)
            {
                return new ValidationResult {Statement = statement, ErrorType = ErrorType.Error, MalformedContractError = error};
            }

            public static ValidationResult CreateWarning(ICSharpStatement statement, string message)
            {
                return new ValidationResult {Statement = statement, ErrorType = ErrorType.Warning, Message = message};
            }
        }

        private static List<Func<ProcessedStatement, ValidationResult>> _malformStatementDetectors = 
            GetMalformStatementDetectors().ToList();

        private static IEnumerable<Func<ProcessedStatement, ValidationResult>> GetMalformStatementDetectors()
        {
            yield return
                s =>
                {
                    // Or boy, I NEED Discriminated union here!!!
                    if (s.ContractStatement == null &&
                        !IsMarkedWithContractValidationAttributeMethod(s.CSharpStatement) && 
                        IsVoidReturnMethod(s.CSharpStatement))
                        return ValidationResult.CreateError(s.CSharpStatement, MalformedContractError.VoidReturnMethodCall);
                    return ValidationResult.NoError;
                };

            yield return
                s =>
                {
                    if (s.ContractStatement != null &&
                        (s.ContractStatement.StatementType == CodeContractStatementType.Assert ||
                        s.ContractStatement.StatementType == CodeContractStatementType.Assume))
                        return ValidationResult.CreateError(s.CSharpStatement, MalformedContractError.AssertOrAssumeInContractBlock);
                    return ValidationResult.NoError;
                };

            yield return
                s =>
                {
                    if (s.ContractStatement == null && IsAssignmentStatement(s.CSharpStatement))
                        return ValidationResult.CreateError(s.CSharpStatement,
                            MalformedContractError.AssignmentInContractBlock);
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

        protected override void Run(ICSharpFunctionDeclaration element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var contractBlockStatements = element.GetContractBlockStatements();

            foreach (var vr in ValidateContractBlockStatements(contractBlockStatements)
                        .Where(v => v.ErrorType != ErrorType.NoError))
            {
                if (vr.ErrorType == ErrorType.Error)
                {
                    consumer.AddHighlighting(
                        new MalformedMethodContractHighlighting(vr.MalformedContractError, element.DeclaredName),
                        vr.Statement.GetDocumentRange(), element.GetContainingFile());
                }
            }
        }

        private static bool IsAssignmentStatement(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            return statement.With(x => x as IExpressionStatement)
                    .With(x => x.Expression as IAssignmentExpression) != null;
        }

        private IEnumerable<ValidationResult> ValidateContractBlockStatements(
            IEnumerable<ProcessedStatement> contractBlockStatements)
        {
            var query = 
                from statement in contractBlockStatements
                let validationResult = ValidateStatement(statement)
                select validationResult;

            return query.ToList();
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