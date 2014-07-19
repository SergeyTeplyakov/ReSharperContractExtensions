using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings.Storage.Persistence;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Warns if method contract is malformed:
    /// - Contract statements are not the first statements in the method
    /// - Ensures statement is after precondition check
    /// </summary>
    [ElementProblemAnalyzer(new[] { typeof(ICSharpFunctionDeclaration) },
    HighlightingTypes = new[] { typeof(MalformedMethodContractHighlighting) })]
    public sealed class MalformedMethodContractChecker : ElementProblemAnalyzer<ICSharpFunctionDeclaration>
    {
        protected override void Run(ICSharpFunctionDeclaration element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var processedStatements = ProcessStatements(element);
            ICSharpStatement faultedStatement;

            if (VoidMethodCallDetectedBeforeMethodContract(processedStatements, out faultedStatement))
            {
                consumer.AddHighlighting(
                    new MalformedMethodContractHighlighting(), 
                    faultedStatement.GetDocumentRange(), element.GetContainingFile());
            }
        }

        //private IEnumerable<IHighlighting> DoRun(ICSharpFunctionDeclaration element)
        //{
        //    var processedStatements = ProcessStatements(element);

        //    if (VoidMethodCallDetectedBeforeMethodContract(processedStatements, out ))
        //    {
        //        yield return new MalformedMethodContractHighlighting();
        //    }

        //    yield break;
        //}

        private bool VoidMethodCallDetectedBeforeMethodContract(IList<ProcessedStatement> processedStatements, 
            out ICSharpStatement faultedStatement)
        {
            bool voidReturnMethodFound = false;
            ICSharpStatement voidStatement = null;
            foreach (var statement in processedStatements)
            {
                if (!voidReturnMethodFound && IsNotContractAndVoidReturnMethod(statement))
                {
                    voidReturnMethodFound = true;
                    voidStatement = statement.Statement;
                }

                if (voidReturnMethodFound && IsContractRequiresEnsuresOrEndContractBlock(statement))
                {
                    faultedStatement = voidStatement; 
                    return true;
                }
            }

            faultedStatement = null;
            return false;
        }

        private bool IsContractRequiresEnsuresOrEndContractBlock(ProcessedStatement statement)
        {
            if (statement.Assertion == null)
                return false;

            // TODO: this is terrible!!!! Rethink assertions and stuff!
            return statement.Assertion is ContractRequiresStatement || 
                statement.Assertion is ContractEnsuresStatement ||
                statement.Assertion is EndContractBlockStatement;
        }

        private bool IsNotContractAndVoidReturnMethod(ProcessedStatement statement)
        {
            if (statement.Assertion != null)
                return false;

            var declaredElement = 
                statement.Statement
                .With(x => x as IExpressionStatement)
                .With(x => x.Expression as IInvocationExpression)
                .With(x => x.InvocationExpressionReference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement as IMethod);

            return declaredElement != null ? declaredElement.ReturnType.IsVoid() : false;
        }

        private IList<ProcessedStatement> ProcessStatements(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.Body
                .Return(x => x.Statements.AsEnumerable(), Enumerable.Empty<ICSharpStatement>())
                .Select(ProcessedStatement.Create)
                .ToList();
        }
    }

    internal class ProcessedStatement
    {
        public ICSharpStatement Statement { get; private set; }
        public ContractStatementBase Assertion { get; private set; }

        public static ProcessedStatement Create(ICSharpStatement statement)
        {
            return new ProcessedStatement
            {
                Statement = statement,
                Assertion = ContractStatementFactory.FromCSharpStatement(statement),
            };
        }
    }
}