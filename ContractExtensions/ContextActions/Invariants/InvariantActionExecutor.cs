using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.ContractUtils;

namespace ReSharper.ContractExtensions.ContextActions.Invariants
{
    internal sealed class InvariantActionExecutor : ContextActionExecutorBase
    {
        private readonly InvariantAvailability _invariantAvailability;
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly IClassLikeDeclaration _classDeclaration;

        public InvariantActionExecutor(InvariantAvailability invariantAvailability,
            ICSharpContextActionDataProvider provider)
            : base(provider)
        {
            Contract.Requires(invariantAvailability != null);
            Contract.Requires(provider != null);
            Contract.Requires(invariantAvailability.IsAvailable);

            _invariantAvailability = invariantAvailability;
            _provider = provider;
            // TODO: look at this class CSharpStatementNavigator

            _classDeclaration = provider.GetSelectedElement<IClassLikeDeclaration>(true, true);
            
            Contract.Assert(provider.SelectedElement != null);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_invariantAvailability != null);
            Contract.Invariant(_classDeclaration != null);
        }

        protected override void DoExecuteTransaction()
        {
            var invariantMethod = AddObjectInvariantMethodIfNecessary();

            var invariantStatement = CreateInvariantStatement();
            var addAfter = GetPreviousInvariantStatement();

            AddStatementAfter(invariantMethod, invariantStatement, addAfter);
        }

        private void AddStatementAfter(IMethodDeclaration method, 
            ICSharpStatement statement, ICSharpStatement addAfter)
        {
            method.Body.AddStatementAfter(statement, addAfter);
        }

        /// <summary>
        /// Returns statement after which current invariant should be added.
        /// </summary>
        /// <remarks>
        /// Order of the invariants:
        /// Invariants that check fields
        /// Invariants that check properties
        /// All of them would be in the order of the declaration.
        /// </remarks>
        [CanBeNull]
        private ICSharpStatement GetPreviousInvariantStatement()
        {
            var declaration = _invariantAvailability.FieldOrPropertyDeclaration;

            IEnumerable<IDeclaration> fields = _classDeclaration.MemberDeclarations.OfType<IFieldDeclaration>();
            IEnumerable<IDeclaration> properties = Enumerable.Empty<IDeclaration>();
            if (declaration.IsProperty)
            {
                properties = _classDeclaration.MemberDeclarations.OfType<IPropertyDeclaration>();
            }

            var members = fields.ToList();
            members.AddRange(properties);

            // Creating lookup where key is argument name, and the value is statements.
            var assertions = _classDeclaration.GetInvariants().ToList();

            // We should consider only members, declared previously to current member
            var previousInvariants =
                members
                    .Select(p => p.DeclaredName)
                    .TakeWhile(paramName => paramName != _invariantAvailability.SelectedMemberName)
                    .Reverse().ToList();

            // Looking for the last usage of the parameters in the requires statements
            foreach (var p in previousInvariants)
            {
                var assertion = assertions.LastOrDefault(a => a.ChecksForNotNull(p));
                if (assertion != null)
                {
                    return assertion.CSharpStatement;
                }
            }

            return null;
        }

        private IMethodDeclaration AddObjectInvariantMethodIfNecessary()
        {
            Contract.Ensures(Contract.Result<IMethodDeclaration>() != null);
            Contract.Ensures(Contract.Result<IMethodDeclaration>().Body != null);

            var invariantMethod = _classDeclaration.GetInvariantMethod();
            if (invariantMethod == null)
                invariantMethod = CreateAndAddObjectInvariantMethod();

            if (!invariantMethod.IsObjectInvariantMethod())
                AddContractInvariantAttribute(invariantMethod);

            return invariantMethod;
        }

        private void AddContractInvariantAttribute(IMethodDeclaration method)
        {
            ITypeElement type = TypeFactory.CreateTypeByCLRName(
                typeof(ContractInvariantMethodAttribute).FullName,
                _provider.PsiModule, _currentFile.GetResolveContext()).GetTypeElement();

            var attribute = _factory.CreateAttribute(type);

            method.AddAttributeBefore(attribute, null);
        }

        [System.Diagnostics.Contracts.Pure]
        private IMethodDeclaration CreateAndAddObjectInvariantMethod()
        {
            Contract.Ensures(Contract.Result<IMethodDeclaration>() != null);

            var method = (IMethodDeclaration)_factory.CreateTypeMemberDeclaration(
                string.Format("private void {0}() {{}}", InvariantUtils.InvariantMethodName),
                EmptyArray<object>.Instance);

            var anchor = GetAnchorForObjectInvariantMethod();

            _classDeclaration.AddClassMemberDeclarationAfter(method, anchor);

            // To enable method modification, method from class declaration should be returned.
            return _classDeclaration.GetInvariantMethod();
        }

        /// <summary>
        /// Returns "acnhor" for contract method: declaration after which ObjectInvariant should be added.
        /// </summary>
        /// <remarks>
        /// ObjectInvariant method should be added after last contructor or after last field declaration.
        /// </remarks>
        [CanBeNull]
        private IClassMemberDeclaration GetAnchorForObjectInvariantMethod()
        {
            var lastConstructor = _classDeclaration.ConstructorDeclarations.LastOrDefault();
            if (lastConstructor != null)
                return lastConstructor;

            return _classDeclaration.MemberDeclarations
                .LastOrDefault(md => md is IFieldDeclaration) as IClassMemberDeclaration;
        }

        private ICSharpStatement CreateInvariantStatement()
        {
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);

            string format = "$0.Invariant($1 != null);";

            return _factory.CreateStatement(format,
                ContractType, _invariantAvailability.SelectedMemberName);
        }
    }
}