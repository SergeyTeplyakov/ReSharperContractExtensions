using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [ElementProblemAnalyzer(new[] { typeof(IInvocationExpression) },
        HighlightingTypes = new[] { typeof(RequiresExceptionInconsistentVisibiityHighlighting) })]
    public sealed class RequiresExceptionInconsistentVisibilityChecker : ElementProblemAnalyzer<IInvocationExpression>
    {
        protected override void Run(IInvocationExpression element, ElementProblemAnalyzerData data, 
            IHighlightingConsumer consumer)
        {
            IClass exceptionDeclaration;
            MemberWithAccess preconditionContainer;
            if (IsAvailable(element, out exceptionDeclaration, out preconditionContainer))
            {
                consumer.AddHighlighting(
                    new RequiresExceptionInconsistentVisibiityHighlighting(exceptionDeclaration, preconditionContainer),
                    element.GetDocumentRange(), element.GetContainingFile());
            }
        }

        private bool IsAvailable(IInvocationExpression expression, 
            out IClass lessVisibleException, out MemberWithAccess preconditionContainer)
        {
            Contract.Ensures(!Contract.Result<bool>() || 
                (Contract.ValueAtReturn(out lessVisibleException) != null && Contract.ValueAtReturn(out preconditionContainer) != null),
                "If the method returns true, all output arguments should not be null");

            lessVisibleException = null;
            preconditionContainer = null;

            var contractAssertion = ContractAssertionExpression.FromInvocationExpression(expression) as ContractRequiresExpression;
            if (contractAssertion == null || 
                contractAssertion.AssertionType != AssertionType.Precondition || 
                contractAssertion.GenericArgumentType == null)
                return false;

            var preconditionHolder =
                expression.GetContainingStatement()
                    .With(x => x.GetContainingTypeMemberDeclaration())
                    .With(x => x.DeclaredElement)
                    .With(MemberWithAccess.FromDeclaredElement);

            preconditionContainer = preconditionHolder;

            var exception = contractAssertion.GenericArgumentDeclaredType
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement as IClass);

            if (exception == null || preconditionContainer == null)
                return false;

            if (!AccessVisibilityChecker.MemberWith(exception.GetAccessRights())
                .IsAccessibleFrom(preconditionHolder.GetCombinedAccessRights()))
            {
                lessVisibleException = exception;
                return true;
            }

            return false;
        }
    }
}