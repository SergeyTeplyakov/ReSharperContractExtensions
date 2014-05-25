using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.reflection2.elements.Compiled;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    internal sealed class EnsuresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly ICSharpFunctionDeclaration _selectedFunction;

        private EnsuresAvailability()
        {}

        public EnsuresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            _provider = provider;

            IsAvailable = ComputeIsAvailable(out _selectedFunction);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || SelectedFunction != null);
        }

        public bool IsAvailable { get; private set; }
        public ICSharpFunctionDeclaration SelectedFunction { get { return _selectedFunction; } }

        public static readonly EnsuresAvailability Unavailable = new EnsuresAvailability {IsAvailable = false};

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

            // For abstract and interface methods contract method differs from the
            // current method
            var contractMethod = methodDeclaration.GetContractFunction();
            if (contractMethod == null)
                return false;

            if (ResultIsAlreadyCheckedByContractEnsures(contractMethod))
                return false;

            if (methodDeclaration.GetReturnType().IsReferenceOrNullableType())
                return true;

            return false;
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

        [Pure]
        private bool IsPropertyWithGetterSelected()
        {
            var propertyDeclaration = _provider.GetSelectedElement<IPropertyDeclaration>(true, true);
            if (propertyDeclaration == null)
                return false;

            if (propertyDeclaration.IsAuto)
                return false;

            return propertyDeclaration.AccessorDeclarations.Any(a => a.Kind == AccessorKind.GETTER);
        }

        private ICSharpFunctionDeclaration GetFunctionDeclaration()
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

        private ICSharpStatement GetLastRequiresStatementIfAny(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.GetRequires().Select(r => r.Statement).LastOrDefault();
        }

        private void AddStatementAfter(ICSharpFunctionDeclaration functionDeclaration,
            ICSharpStatement statement, ICSharpStatement anchor)
        {
            functionDeclaration.Body.AddStatementAfter(statement, anchor);
        }

        private bool ResultIsAlreadyCheckedByContractEnsures(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            if (functionDeclaration.Body == null)
                return false;

            var returnType = functionDeclaration.GetReturnType();
            return functionDeclaration.GetEnsures()
                .Any(e => e.ResultType.GetClrName().FullName == returnType.GetClrName().FullName);
        }

        private bool ResultIsVoid(IFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.GetReturnType().IsVoid();
        }

    }
}