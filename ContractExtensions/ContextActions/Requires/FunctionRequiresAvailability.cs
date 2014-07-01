using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractsEx;
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
            Contract.Requires(parameterName != null);

            ParameterName = parameterName;
            
            _functionToInsertPrecondition = selectedFunction ?? GetSelectedFunctionDeclaration();
            _isAvailable = IsRequiresAvailable(parameterName, ref _functionToInsertPrecondition);
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

        [Pure]
        private bool IsRequiresAvailable(string parameterName, ref ICSharpFunctionDeclaration functionDeclaration)
        {
            var functionToInsertPrecondition = functionDeclaration.GetContractFunction();

            if (!functionToInsertPrecondition.IsValidForContracts())
                return false;

            if (ArgumentIsAlreadyVerifiedByArgCheckOrRequires(functionToInsertPrecondition, parameterName))
                return false;

            functionDeclaration = functionToInsertPrecondition;

            return true;
        }

        [Pure]
        private ICSharpFunctionDeclaration GetSelectedFunctionDeclaration()
        {
            return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
        }

        [Pure]
        private bool ArgumentIsAlreadyVerifiedByArgCheckOrRequires(
            ICSharpFunctionDeclaration functionDeclaration, string parameterName)
        {
            return functionDeclaration.GetPreconditions()
                .Any(p => p.AssertsArgumentIsNotNull(parameterName));
        }
    }
}