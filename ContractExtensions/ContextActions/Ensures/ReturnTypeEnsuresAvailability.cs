using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    /// <summary>
    /// Computes whether adding Contract.Ensures should be available based
    /// on the return type of the select method or property.
    /// </summary>
    internal sealed class ReturnTypeEnsuresAvailability
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

        private bool ComputeIsAvailable(out ICSharpFunctionDeclaration currentFunction)
        {
            currentFunction = null;

            if (!IsReturnOrMethodDeclarationSelected() && !IsPropertyWithGetterSelected())
                return false;

            var methodDeclaration = GetFunctionDeclaration();
            currentFunction = methodDeclaration;

            if (methodDeclaration == null || methodDeclaration.DeclaredElement == null)
                return false;

            if (ResultIsVoid(methodDeclaration))
                return false;

            if (!methodDeclaration.GetReturnType().IsReferenceOrNullableType())
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
                return _provider.GetSelectedElement<IPropertyDeclaration>(true, true)
                    .AccessorDeclarations.First(d => d.Kind == AccessorKind.GETTER);
            }

            Contract.Assert(false, "Impossible situation");
            throw new InvalidOperationException();
        }

        [Pure] private bool ResultIsVoid(IFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.GetReturnType().IsVoid();
        }
    }
}