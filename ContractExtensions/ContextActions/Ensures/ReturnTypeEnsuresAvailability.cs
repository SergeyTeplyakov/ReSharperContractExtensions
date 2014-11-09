using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    /// <summary>
    /// Computes whether adding Contract.Ensures should be available based
    /// on the return type of the select method or property.
    /// </summary>
    [ContractClass(typeof (ReturnTypeEnsuresAvailabilityContract))]
    internal abstract class ReturnTypeEnsuresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly ICSharpFunctionDeclaration _currentFunction;

        public ReturnTypeEnsuresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;

            IsAvailable = ComputeIsAvailable(out _currentFunction);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _currentFunction != null);
        }

        public bool IsAvailable { get; private set; }
        public ICSharpFunctionDeclaration SelectedFunctionDeclaration { get { return _currentFunction; } }

        protected abstract bool ReturnTypeCompatibleForEnsure(ICSharpFunctionDeclaration methodDeclaration);

        private bool ComputeIsAvailable(out ICSharpFunctionDeclaration currentFunction)
        {
            currentFunction = null;

            if (!IsReturnOrMethodDeclarationSelected() && !IsPropertyWithGetterSelected())
                return false;

            var methodDeclaration = GetFunctionDeclaration();

            currentFunction = methodDeclaration;

            if (!methodDeclaration.IsValidForContracts())
                return false;

            if (ResultIsVoid(methodDeclaration))
                return false;

            if (!ReturnTypeCompatibleForEnsure(methodDeclaration))
                return false;

            return true;
        }

        [Pure]
        private bool IsReturnOrMethodDeclarationSelected()
        {
            if (_provider.SelectedElement == null)
                return false;

            // Disable on parameters
            if (_provider.IsSelected<IParameterDeclaration>())
                return false;

            if (!_provider.IsSelected<IMethodDeclaration>())
                return false;

            // Enable on return statement
            if (_provider.IsSelected<IReturnStatement>())
                return true;

            // Ensures enable only on return statement in the method body
            if (_provider.IsSelected<IChameleonNode>())
                return false;

            return true;
        }

        [Pure] private bool IsPropertyWithGetterSelected()
        {
            // Ensures is available for indexer, but should not be available for selected setters
            var indexedPropertyDeclaration = _provider.GetSelectedElement<IIndexerDeclaration>(true, true);
            if (indexedPropertyDeclaration != null)
            {
                var selectedAccessor = _provider.GetSelectedElement<IAccessorDeclaration>(true, true);

                var getter = indexedPropertyDeclaration.AccessorDeclarations.FirstOrDefault(a => a.Kind == AccessorKind.GETTER);
                if (getter != null)
                {
                    if (selectedAccessor == null || selectedAccessor == getter)
                        return true;
                }
            }

            var propertyDeclaration = _provider.GetSelectedElement<IPropertyDeclaration>(true, true);
            if (propertyDeclaration == null)
                return false;

            if (propertyDeclaration.IsAuto)
                return false;

            return propertyDeclaration.AccessorDeclarations.Any(a => a.Kind == AccessorKind.GETTER);
        }

        [Pure] private ICSharpFunctionDeclaration GetFunctionDeclaration()
        {
            Contract.Requires(IsReturnOrMethodDeclarationSelected() || IsPropertyWithGetterSelected());

            Contract.Ensures(Contract.Result<ICSharpFunctionDeclaration>() != null);
            Contract.Ensures(Contract.Result<ICSharpFunctionDeclaration>().DeclaredElement != null);

            if (IsReturnOrMethodDeclarationSelected())
                return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);

            if (IsPropertyWithGetterSelected())
            {
                var propertyOwner = (IAccessorOwnerDeclaration)_provider.GetSelectedElement<IIndexerDeclaration>(true, true) ??
                              _provider.GetSelectedElement<IPropertyDeclaration>(true, true);
                
                Contract.Assert(propertyOwner != null);

                return propertyOwner.AccessorDeclarations.First(d => d.Kind == AccessorKind.GETTER);
            }

            Contract.Assert(false, "Impossible situation");
            throw new InvalidOperationException();
        }

        [Pure]
        private bool ResultIsVoid(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.GetReturnType().IsVoid();
        }
    }

    [ContractClassFor(typeof (ReturnTypeEnsuresAvailability))]
    abstract class ReturnTypeEnsuresAvailabilityContract : ReturnTypeEnsuresAvailability
    {
        protected ReturnTypeEnsuresAvailabilityContract(ICSharpContextActionDataProvider provider) 
            : base(provider)
        {}

        protected override bool ReturnTypeCompatibleForEnsure(ICSharpFunctionDeclaration methodDeclaration)
        {
            Contract.Requires(methodDeclaration != null);
            throw new NotImplementedException();
        }
    }

    internal sealed class NullCheckReturnTypeEnsuresAvailability : ReturnTypeEnsuresAvailability
    {
        public NullCheckReturnTypeEnsuresAvailability(ICSharpContextActionDataProvider provider) 
            : base(provider)
        {}

        protected override bool ReturnTypeCompatibleForEnsure(ICSharpFunctionDeclaration methodDeclaration)
        {
            return methodDeclaration.GetReturnType().IsReferenceOrNullableType();
        }
    }

    internal sealed class EnumCheckReturnTypeEnsuresAvailability : ReturnTypeEnsuresAvailability
    {
        public EnumCheckReturnTypeEnsuresAvailability(ICSharpContextActionDataProvider provider) : base(provider)
        {}

        protected override bool ReturnTypeCompatibleForEnsure(ICSharpFunctionDeclaration methodDeclaration)
        {
            var returnType = methodDeclaration.GetReturnType();
            return returnType.IsEnumType() || 
                (returnType.IsNullable() && returnType.GetNullableUnderlyingType().IsEnumType());
        }
    }
}