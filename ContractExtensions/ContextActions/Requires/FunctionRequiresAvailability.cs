using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Checks that Contract.Requires should be available for specified function and parameter name.
    /// </summary>
    // TODO: name is really poor!!
    internal sealed class FunctionRequiresAvailability : ContextActionAvailabilityBase<FunctionRequiresAvailability>
    {
        /// <summary>
        /// Function, where the tool should insert precondition. Note that this could be different
        /// from the selected function (because selected function could be in the abstract class).
        /// </summary>
        private readonly ICSharpFunctionDeclaration _functionToInsertPrecondition;

        public FunctionRequiresAvailability() { }

        public FunctionRequiresAvailability(ICSharpContextActionDataProvider provider, string parameterName, 
            ICSharpFunctionDeclaration selectedFunction = null)
            : base(provider)
        {
            Contract.Requires(provider != null);
            Contract.Requires(!string.IsNullOrEmpty(parameterName));

            ParameterName = parameterName;
            
            _functionToInsertPrecondition = GetContractFunction(selectedFunction);
            
            if (_functionToInsertPrecondition != null)
                _isAvailable = IsRequiresAvailable(parameterName, _functionToInsertPrecondition);
        }

        [CanBeNull]
        private ICSharpFunctionDeclaration GetContractFunction(ICSharpFunctionDeclaration selectedFunction)
        {
            return (selectedFunction ?? GetSelectedFunctionDeclaration()).GetContractFunction();
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _functionToInsertPrecondition != null);
            Contract.Invariant(!IsAvailable || ParameterName != null);
        }

        public string ParameterName { get; private set; }
        public ICSharpFunctionDeclaration FunctionToInsertPrecondition { get { return _functionToInsertPrecondition; } }

        [System.Diagnostics.Contracts.Pure]
        private bool IsRequiresAvailable(string parameterName, ICSharpFunctionDeclaration functionToInsertPrecondition)
        {
            Contract.Requires(!string.IsNullOrEmpty(parameterName));
            Contract.Requires(functionToInsertPrecondition != null);

            if (!functionToInsertPrecondition.IsValidForContracts())
                return false;

            if (IsArgumentAlreadyVerifiedByPrecondition(functionToInsertPrecondition, parameterName))
                return false;

            return true;
        }

        [System.Diagnostics.Contracts.Pure]
        private ICSharpFunctionDeclaration GetSelectedFunctionDeclaration()
        {
            return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
        }

        [System.Diagnostics.Contracts.Pure]
        private bool IsArgumentAlreadyVerifiedByPrecondition(
            ICSharpFunctionDeclaration functionDeclaration, string parameterName)
        {
            return functionDeclaration.GetPreconditions()
                .Any(p => p.ChecksForNotNull(parameterName));
        }
    }
}