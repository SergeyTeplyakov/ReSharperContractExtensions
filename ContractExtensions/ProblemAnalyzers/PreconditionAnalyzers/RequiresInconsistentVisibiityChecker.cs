using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
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
    internal class MemberWithAccess
    {
        public MemberWithAccess(string memberName, string memberType, AccessRights accessRights)
        {
            MemberName = memberName;
            MemberType = memberType;
            AccessRights = accessRights;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(MemberName != null);
            Contract.Invariant(MemberType != null);
        }

        public string MemberName { get; private set; }
        public string MemberType { get; private set; }
        public AccessRights AccessRights { get; private set; }
    }


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
            MemberWithAccess enclosingMember;
            if (IsAvailable(element, out preconditionContainer, out enclosingMember))
            {
                consumer.AddHighlighting(
                    new RequiresInconsistentVisibiityHighlighting(element.GetContainingStatement(), preconditionContainer, enclosingMember),
                    element.GetDocumentRange(), element.GetContainingFile());
            }
        }

        private bool IsAvailable(IInvocationExpression expression, out MemberWithAccess preconditionContainer, 
            out MemberWithAccess enclosingMember)
        {
            preconditionContainer = null;
            enclosingMember = null;

            var contractAssertion = ContractAssertionExpression.FromInvocationExpression(expression);
            if (contractAssertion == null)
                return false;
            
            var preconditionHolder =
                expression.GetContainingStatement()
                    .With(x => x.GetContainingTypeMemberDeclaration())
                    .With(x => new MemberWithAccess(x.DeclaredName, "foo", x.GetAccessRights()));

            preconditionContainer = preconditionHolder;
            if (preconditionContainer == null)
                return false;

            enclosingMember = ProcessExpression(contractAssertion.PredicateExpression)
                .FirstOrDefault(member => 
                !AccessVisibilityChecker.MemberWith(member.AccessRights)
                    .IsAccessibleFrom(preconditionHolder.AccessRights));

            return enclosingMember != null;
        }

        [Pure]
        private IEnumerable<MemberWithAccess> ProcessExpression(IExpression expression)
        {
            foreach (var reference in expression.ProcessRecursively<IReferenceExpression>())
            {
                var memberWithAccess =
                    reference
                    .With(x => x.Reference)
                    .With(x => x.Resolve())
                    .With(x => x.DeclaredElement)
                    .With(x => x as IAccessRightsOwner)
                    .With(x => new MemberWithAccess(
                        ((IDeclaredElement)x).ShortName, GetMemberType((IDeclaredElement)x), x.GetAccessRights()));

                if (memberWithAccess != null)
                    yield return memberWithAccess;

            }
        }

        public static string GetMemberType(IDeclaredElement element)
        {
            if (element is IProperty)
                return "property";
            if (element is IMethod)
                return "method";
            if (element is IField)
                return "field";

            return "contractHolder";
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

    [ConfigurableSeverityHighlighting("RequiresInconsistentVisibilityHighlighting", CSharpLanguage.Name)]
    public sealed class RequiresInconsistentVisibiityHighlighting : IHighlighting
    {
        private readonly ICSharpStatement _preconditionStatement;
        private readonly MemberWithAccess _preconditionContainer;
        private readonly MemberWithAccess _enclosingMember;

        internal RequiresInconsistentVisibiityHighlighting(ICSharpStatement preconditionStatement, 
            MemberWithAccess preconditionContainer, MemberWithAccess enclosingMember)
        {
            Contract.Requires(preconditionStatement != null);
            Contract.Requires(preconditionContainer != null);
            Contract.Requires(enclosingMember != null);

            _preconditionStatement = preconditionStatement;
            _preconditionContainer = preconditionContainer;
            _enclosingMember = enclosingMember;
        }

        public const string ServerityId = "RequiresInconsistentVisibilityHighlighting";
        // Sample: Member 'ConsoleApplication6.Demo.Analyzers.Demo.Check' has less visibility than the enclosing method 'ConsoleApplication6.Demo.Analyzers.Demo.Foo(System.String)'.
        const string ToolTipWarningFormat = "Member '{0}' has less visibility than the enclosing {1} '{2}'";

        public bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return string.Format(ToolTipWarningFormat, _preconditionContainer.MemberName, 
                _enclosingMember.MemberType, _enclosingMember.MemberName); }
        }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }
    }

    public sealed class AccessVisibilityChecker
    {
        private readonly AccessRights _enclosingMember;

        private AccessVisibilityChecker(AccessRights enclosingMember)
        {
            _enclosingMember = enclosingMember;
        }


        public static AccessVisibilityChecker MemberWith(AccessRights enclosingMember)
        {
            return new AccessVisibilityChecker(enclosingMember);
        }

        public bool IsAccessibleFrom(AccessRights preconditionHolder)
        {
            if (_accessRightsCompatibility.ContainsKey(preconditionHolder))
            {
                return _accessRightsCompatibility[preconditionHolder](_enclosingMember);
            }

            Contract.Assert(false, string.Format("Unknown enclosing member visibility: {0}", _enclosingMember));
            return false;
        }

        private static readonly Dictionary<AccessRights, Func<AccessRights, bool>> _accessRightsCompatibility = FillRules();

        [Pure]
        private static Dictionary<AccessRights, Func<AccessRights, bool>> FillRules()
        {
            var accessRights = new Dictionary<AccessRights, Func<AccessRights, bool>>();
            accessRights[AccessRights.PUBLIC] = ar => ar == AccessRights.PUBLIC;

            accessRights[AccessRights.PROTECTED] = 
                accessRights[AccessRights.PUBLIC]
                .Or(ar => ar == AccessRights.PROTECTED_OR_INTERNAL)
                .Or(ar => ar == AccessRights.PROTECTED);

            accessRights[AccessRights.INTERNAL] = 
                accessRights[AccessRights.PUBLIC]
                .Or(ar => ar == AccessRights.PROTECTED_OR_INTERNAL)
                .Or(ar => ar == AccessRights.INTERNAL);

            accessRights[AccessRights.PROTECTED_OR_INTERNAL] =
                accessRights[AccessRights.PUBLIC]
                .Or(accessRights[AccessRights.PROTECTED])
                .Or(accessRights[AccessRights.INTERNAL]);


            accessRights[AccessRights.PROTECTED_AND_INTERNAL] = 
                accessRights[AccessRights.PUBLIC]
                .Or(ar => ar == AccessRights.PROTECTED_AND_INTERNAL);

            accessRights[AccessRights.PRIVATE] = ar => true;
            accessRights[AccessRights.NONE] = ar => false;

            return accessRights;
        }
    }

}