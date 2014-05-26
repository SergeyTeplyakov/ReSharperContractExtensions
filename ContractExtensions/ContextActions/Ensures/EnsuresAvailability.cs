using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.reflection2.elements.Compiled;
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
            if (!EnsuresAvailableForSelectedReturnType(out currentFunction))
                return false;

            // For abstract and interface methods contract method differs from the
            // current method
            var contractMethod = currentFunction.GetContractFunction();
            if (contractMethod == null)
                return false;

            if (ResultIsAlreadyCheckedByContractEnsures(contractMethod))
                return false;

            return true;
        }

        private bool EnsuresAvailableForSelectedReturnType(out ICSharpFunctionDeclaration selectedFunction)
        {
            var availability = new ReturnTypeEnsuresAvailability(_provider);
            selectedFunction = availability.SelectedFunctionDeclaration;
            return availability.IsAvailable;
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
    }
}