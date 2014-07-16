using System;
using System.Diagnostics.Contracts;
using System.Linq;
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
        HighlightingTypes = new[] {
            typeof(RequiresExceptionInconsistentVisibiityHighlighting),
            typeof(RequiresExceptionValidityHighlighting) })]
    public sealed class RequiresExceptionInconsistentVisibilityChecker : ElementProblemAnalyzer<IInvocationExpression>
    {
        protected override void Run(IInvocationExpression element, ElementProblemAnalyzerData data, 
            IHighlightingConsumer consumer)
        {
            IClass exception;
            MemberWithAccess preconditionContainer;
            ContractRequiresExpression contractAssertion;

            if (!ParseInvocationExpression(element, out contractAssertion, out preconditionContainer, out exception))
                return;

            if (ExceptionIsLessVisible(preconditionContainer, exception))
            {
                consumer.AddHighlighting(
                    new RequiresExceptionInconsistentVisibiityHighlighting(exception, preconditionContainer),
                    element.GetDocumentRange(), element.GetContainingFile());
            }


            if (DoesntHaveAppropriateConstructor(exception))
            {
                consumer.AddHighlighting(
                    new RequiresExceptionValidityHighlighting(exception, preconditionContainer),
                    element.GetDocumentRange(), element.GetContainingFile());
            }
        }

        private bool ParseInvocationExpression(IInvocationExpression invocationExpression, 
            out ContractRequiresExpression requiresExpression,
            out MemberWithAccess preconditionContainer, out IClass preconditionException)
        {
            Contract.Ensures(!Contract.Result<bool>() || 
                (Contract.ValueAtReturn(out requiresExpression) != null) &&
                (Contract.ValueAtReturn(out preconditionContainer) != null) &&
                (Contract.ValueAtReturn(out preconditionException) != null),
                "If method returns true, all out arguments should be well-defined!");

            preconditionContainer = null;
            preconditionException = null;

            requiresExpression = ContractAssertionExpression.FromInvocationExpression(invocationExpression) 
                as ContractRequiresExpression;

            if (requiresExpression == null ||
                requiresExpression.AssertionType != AssertionType.Precondition ||
                requiresExpression.GenericArgumentType == null)
                return false;

            preconditionContainer =
                invocationExpression
                    .With(x => x.GetContainingStatement())
                    .With(x => x.GetContainingTypeMemberDeclaration())
                    .With(x => x.DeclaredElement)
                    .With(MemberWithAccess.FromDeclaredElement);

            preconditionException = requiresExpression.GenericArgumentDeclaredType
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement as IClass);

            return preconditionContainer != null && preconditionException != null;
        }

        /// <summary>
        /// Returns true if exception type in the Contract.Requires&lt;E&gt; doesn't have
        /// appropriate constructors: ctor(string) or ctor(string, string).
        /// </summary>
        private bool DoesntHaveAppropriateConstructor(IClass exceptionDeclaration)
        {
            Contract.Requires(exceptionDeclaration != null);

            return exceptionDeclaration.Constructors.All(c => !c.IsSutableForGenericContractRequires());
        }

        private bool ExceptionIsLessVisible(MemberWithAccess preconditionContainer, IClass exception)
        {
            Contract.Requires(preconditionContainer != null);
            Contract.Requires(exception != null);

            return !AccessVisibilityChecker.MemberWith(exception.GetAccessRights())
                .IsAccessibleFrom(preconditionContainer.GetCombinedAccessRights());
        }
    }
}