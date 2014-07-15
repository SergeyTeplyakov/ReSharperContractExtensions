using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Intentions.Xaml.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Refactorings.Util;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;
using ReSharper.ContractExtensions.Utilities;

[assembly: RegisterConfigurableSeverity(RequiresInconsistentVisibiityHighlighting.ServerityId,
  null,
  HighlightingGroupIds.CompilerWarnings, // this is actually a Code Contract compiler error CC1038
  RequiresInconsistentVisibiityHighlighting.ServerityId,
  "Warning for inconsistent visibility members in the precondition check",
  Severity.ERROR,
  false)]

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    /// <summary>
    /// Checks inconsistent visibility in Contract.Requires.
    /// </summary>
    /// <remarks>
    /// This class checks for the CC1038 error: 
    /// Member 'ConsoleApplication6.Demo.Analyzers.Demo.Check' has less visibility than the 
    /// enclosing method 'ConsoleApplication6.Demo.Analyzers.Demo.Foo(System.String)'.
    /// </remarks>
    [ElementProblemAnalyzer(new[] { typeof(IInvocationExpression) },
        HighlightingTypes = new[] { typeof(RequiresInconsistentVisibiityHighlighting) })]
    public sealed class RequiresInconsistentVisibiityChecker : ElementProblemAnalyzer<IInvocationExpression>
    {
        protected override void Run(IInvocationExpression element, ElementProblemAnalyzerData data, 
            IHighlightingConsumer consumer)
        {
            MemberWithAccess preconditionContainer;
            MemberWithAccess lessVisibleMember;
            if (IsAvailable(element, out preconditionContainer, out lessVisibleMember))
            {
                consumer.AddHighlighting(
                    new RequiresInconsistentVisibiityHighlighting(
                        element.GetContainingStatement(), preconditionContainer, lessVisibleMember),
                    element.GetDocumentRange(), element.GetContainingFile());
            }
        }

        [System.Diagnostics.Contracts.Pure]
        private bool IsAvailable(IInvocationExpression expression, out MemberWithAccess preconditionContainer, 
            out MemberWithAccess lessVisibleMember)
        {
            preconditionContainer = null;
            lessVisibleMember = null;

            var contractAssertion = ContractAssertionExpression.FromInvocationExpression(expression);
            if (contractAssertion == null || contractAssertion.AssertionType != AssertionType.Precondition)
                return false;
            
            var preconditionHolder =
                expression.GetContainingStatement()
                    .With(x => x.GetContainingTypeMemberDeclaration())
                    .With(x => x.DeclaredElement)
                    .With(MemberWithAccess.FromDeclaredElement);

            preconditionContainer = preconditionHolder;
            if (preconditionContainer == null)
                return false;

            // Looking for a "enclosing" members that are less visible then a contract holder.
            // The only exception is a field with ContractPublicPropertyName attribute.
            lessVisibleMember = 
                ProcessReferenceExpressions(contractAssertion.PredicateExpression)
                .FirstOrDefault(member => 
                !FieldFromPreconditionMarkedWithContractPublicPropertyName(member) && 
                !AccessVisibilityChecker.Member(member).IsAccessibleFrom(preconditionHolder));

            return lessVisibleMember != null;
        }

        [Pure]
        private IEnumerable<MemberWithAccess> ProcessReferenceExpressions(IExpression expression)
        {
            foreach (var reference in expression.ProcessRecursively<IReferenceExpression>())
            {
                var memberWithAccess =
                    reference
                        .With(x => x.Reference)
                        .With(x => x.Resolve())
                        .With(x => x.DeclaredElement)
                        .With(MemberWithAccess.FromDeclaredElement);

                if (memberWithAccess != null)
                    yield return memberWithAccess;
            }
        }

        [System.Diagnostics.Contracts.Pure]
        private bool FieldFromPreconditionMarkedWithContractPublicPropertyName(MemberWithAccess member)
        {
            if (member.MemberType != MemberType.Field)
                return false;

            var field = (IField)member.DeclaredElement;
            return field.HasAttributeInstance(
                new ClrTypeName(typeof (ContractPublicPropertyNameAttribute).FullName), false);
        }
    }

    internal static class FuncEx
    {
        public static Func<T, bool> Or<T>(this Func<T, bool> original, Func<T, bool> another)
        {
            return t => original(t) || another(t);
        }

        public static Func<T, bool> And<T>(this Func<T, bool> original, Func<T, bool> another)
        {
            return t => original(t) && another(t);
        }
    }
}