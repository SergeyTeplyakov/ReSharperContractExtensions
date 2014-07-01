using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Util;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    internal class EnsuresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly ReturnTypeEnsuresAvailability _returnTypeEnsuresAvailability;
        private readonly ICSharpFunctionDeclaration _selectedFunction;

        private EnsuresAvailability()
        {}

        protected EnsuresAvailability(ICSharpContextActionDataProvider provider, 
            ReturnTypeEnsuresAvailability returnTypeEnsuresAvailability)
        {
            Contract.Requires(provider != null);
            Contract.Requires(returnTypeEnsuresAvailability != null);

            _provider = provider;
            _returnTypeEnsuresAvailability = returnTypeEnsuresAvailability;
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

        public static EnsuresAvailability IsAvailableForNullableResult(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            return new EnsuresAvailability(provider, new NullCheckReturnTypeEnsuresAvailability(provider));
        }

        public static EnsuresAvailability IsAvailableForEnumResult(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            return new EnumResultEnsuresAvailability(provider, new EnumCheckReturnTypeEnsuresAvailability(provider));
        }

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
            selectedFunction = _returnTypeEnsuresAvailability.SelectedFunctionDeclaration;
            return _returnTypeEnsuresAvailability.IsAvailable;
        }

        private bool ResultIsAlreadyCheckedByContractEnsures(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);

            if (functionDeclaration.Body == null)
                return false;

            var returnType = functionDeclaration.GetReturnType();

            if (returnType == null)
                return false;

            return ResultIsAlreadyCheckedByContractEnsures(
                functionDeclaration.GetContractEnsures(), returnType);
        }

        protected virtual bool ResultIsAlreadyCheckedByContractEnsures(
            IEnumerable<ContractEnsuresAssertion> ensureAssertions, IDeclaredType methodReturnType)
        {
            Contract.Requires(ensureAssertions != null);
            Contract.Requires(methodReturnType != null);

            return ensureAssertions
                .Any(e => e.AssertsArgumentIsNotNull(
                    pa => pa.With(x => x as ContractResultPredicateArgument)
                        .With(x => x.ResultTypeName.FullName) == methodReturnType.GetClrName().FullName));
        }
    }

    // TODO: names and design are bad!! revise them later!
    internal sealed class EnumResultEnsuresAvailability : EnsuresAvailability
    {
        internal EnumResultEnsuresAvailability(ICSharpContextActionDataProvider provider, ReturnTypeEnsuresAvailability returnTypeEnsuresAvailability) 
            : base(provider, returnTypeEnsuresAvailability)
        {}

        protected override bool ResultIsAlreadyCheckedByContractEnsures(IEnumerable<ContractEnsuresAssertion> ensureAssertions, IDeclaredType methodReturnType)
        {
            // Ensures for enums should be available for System.Enum and for System.Enum?
            // thats why we should extract underlying type out of the nullable type.
            var returnType = methodReturnType.IsNullable() ? methodReturnType.GetNullableUnderlyingType() : methodReturnType;
            return ensureAssertions
                .Any(e => e.AssertsArgumentIsNotNull(
                    pa => pa.With(x => x as ContractResultPredicateArgument)
                        .With(x => x.ResultTypeName.FullName) == returnType.GetClrTypeName().FullName));
        }
    }
}