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

        private static List<Func<ICSharpStatement, ValidationResult>> _malformStatementDetectors = 
            GetMalformStatementDetectors().ToList();

        private static IEnumerable<Func<ICSharpStatement, ValidationResult>> GetMalformStatementDetectors()
        {
            yield return
                s =>
                {
                    if (!IsMarkedWithContractValidationAttributeMethod(s) && IsVoidReturnMethod(s))
                        return ValidationResult.CreateError(s, MalformedContractError.VoidReturnMethodCall);
                    return ValidationResult.NoError;
                };
        }

        private static ValidationResult ValidateStatement(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            Contract.Ensures(Contract.Result<ValidationResult>() != null);

            return _malformStatementDetectors
                .Select(detector => detector(statement))
                .FirstOrDefault(vr => vr.ErrorType != ErrorType.NoError) ?? ValidationResult.CreatNoError(statement);
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
                        new MalformedMethodContractHighlighting(vr.MalformedContractError, element.NameIdentifier.Name),
                        vr.Statement.GetDocumentRange(), element.GetContainingFile());
                }
            }
        }

        private IEnumerable<ValidationResult> ValidateContractBlockStatements(
            IEnumerable<ProcessedStatement> contractBlockStatements)
        {
            var query = 
                from statement in contractBlockStatements
                where statement.ContractStatement == null
                let validationResult = ValidateStatement(statement.CSharpStatement)
                select validationResult;

            return query.ToList();
        } 


        private IEnumerable<List<ICSharpStatement>> VoidMethodCallsInsideContractBlock(IList<ProcessedStatement> processedStatements)
        {
            var faultedStatements = new List<ICSharpStatement>();
            
            foreach (var statement in processedStatements)
            {
                if (IsNotContractAndVoidReturnMethod(statement))
                {
                    faultedStatements.Add(statement.CSharpStatement);
                }
                else if (statement.ContractStatement != null && faultedStatements.Count != 0)
                {
                    yield return faultedStatements;
                    faultedStatements = new List<ICSharpStatement>();
                }
            }
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

        private bool IsNotContractAndVoidReturnMethod(ProcessedStatement statement)
        {
            if (statement.ContractStatement != null)
                return false;

            var declaredElement = 
                statement.CSharpStatement
                .With(x => x as IExpressionStatement)
                .With(x => x.Expression as IInvocationExpression)
                .With(x => x.InvocationExpressionReference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement as IMethod);

            return declaredElement != null ? declaredElement.ReturnType.IsVoid() : false;
        }


    }

    
}