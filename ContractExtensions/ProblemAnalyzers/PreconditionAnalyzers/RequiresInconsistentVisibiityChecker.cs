using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
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
    internal enum MemberType
    {
        Method,
        Property,
        Field,
        Member
    }

    internal sealed class MemberWithAccess
    {
        private readonly IDeclaredElement _declaredElement;

        private MemberWithAccess(IDeclaredElement declaredElement, MemberType memberType, AccessRights accessRights)
        {
            Contract.Requires(declaredElement != null);

            _declaredElement = declaredElement;

            MemberName = declaredElement.ShortName;
            MemberType = memberType;
            AccessRights = accessRights;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(MemberName != null);
            Contract.Invariant(MemberTypeName != null);
        }

        [CanBeNull]
        public static MemberWithAccess FromDeclaredElement(IDeclaredElement declaredElement)
        {
            Contract.Requires(declaredElement != null);

            var accessRightsOwner = declaredElement as IAccessRightsOwner;
            if (accessRightsOwner == null)
                return null;

            return new MemberWithAccess(declaredElement, GetMemberType(declaredElement), 
                accessRightsOwner.GetAccessRights());
        }

        public string MemberName { get; private set; }

        public MemberType MemberType { get; private set; }

        public string MemberTypeName
        {
            get
            {
                switch (MemberType)
                {
                    case MemberType.Method:
                        return "method";
                    case MemberType.Property:
                        return "property";
                    case MemberType.Field:
                        return "field";
                    default:
                        return "member";
                }
            }
        }

        public AccessRights AccessRights { get; private set; }

        public IDeclaredElement DeclaredElement
        {
            get { return _declaredElement; }
        }

        private static MemberType GetMemberType(IDeclaredElement element)
        {
            Contract.Requires(element != null);

            if (element is IProperty)
                return MemberType.Property;
            if (element is IMethod)
                return MemberType.Method;
            if (element is IField)
                return MemberType.Field;

            return MemberType.Member;
        }
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

        [System.Diagnostics.Contracts.Pure]
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
                    .With(x => x.DeclaredElement)
                    .With(MemberWithAccess.FromDeclaredElement);

            preconditionContainer = preconditionHolder;
            if (preconditionContainer == null)
                return false;

            // Looking for a "enclosing" members that are less visible then a contract holder.
            // The only exception is a field with ContractPublicPropertyName attribute.
            enclosingMember = 
                ProcessExpression(contractAssertion.PredicateExpression)
                .FirstOrDefault(member => 
                !EnclosingFieldMarkedWithContractPublicPropertyName(member) && 
                !AccessVisibilityChecker.MemberWith(member.AccessRights).IsAccessibleFrom(preconditionHolder.AccessRights));

            return enclosingMember != null;
        }

        [System.Diagnostics.Contracts.Pure]
        private IEnumerable<MemberWithAccess> ProcessExpression(IExpression expression)
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
        private bool EnclosingFieldMarkedWithContractPublicPropertyName(MemberWithAccess member)
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

        internal MemberWithAccess PreconditionContainer { get { return _preconditionContainer; } }
        internal MemberWithAccess EnclosingMember { get { return _enclosingMember; } }

        public string ToolTip
        {
            get { return string.Format(ToolTipWarningFormat, _preconditionContainer.MemberName, 
                _enclosingMember.MemberTypeName, _enclosingMember.MemberName); }
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

        [System.Diagnostics.Contracts.Pure]
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