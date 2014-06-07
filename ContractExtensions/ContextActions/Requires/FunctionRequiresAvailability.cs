using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractUtils;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Checks that Contract.Requires should be available for specified function and parameter name.
    /// </summary>
    internal sealed class FunctionRequiresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        
        /// <summary>
        /// Function, where the tool should insert precondition. Note that this could be different
        /// from the selected function (because selected function could be in the abstract class).
        /// </summary>
        private readonly ICSharpFunctionDeclaration _functionToInsertPrecondition;

        public FunctionRequiresAvailability(ICSharpContextActionDataProvider provider, string parameterName, 
            ICSharpFunctionDeclaration selectedFunction = null)
        {
            Contract.Requires(provider != null);
            Contract.Requires(parameterName != null);

            _provider = provider;
            ParameterName = parameterName;
            
            _functionToInsertPrecondition = selectedFunction ?? GetSelectedFunctionDeclaration();
            IsAvailable = IsRequiresAvailable(parameterName, ref _functionToInsertPrecondition);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _functionToInsertPrecondition != null);
            Contract.Invariant(!IsAvailable || ParameterName != null);
        }

        public bool IsAvailable { get; private set; }
        public string ParameterName { get; private set; }
        public ICSharpFunctionDeclaration FunctionToInsertPrecondition { get { return _functionToInsertPrecondition; } }

        [Pure]
        private bool IsRequiresAvailable(string parameterName, ref ICSharpFunctionDeclaration functionDeclaration)
        {
            var functionToInsertPrecondition = functionDeclaration.GetContractFunction();

            if (!IsFunctionWellDefined(functionToInsertPrecondition))
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
        private bool IsFunctionWellDefined(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration != null
                   && functionDeclaration.Body != null
                   && functionDeclaration.DeclaredElement != null;
        }

        [Pure]
        private bool ArgumentIsAlreadyVerifiedByArgCheckOrRequires(
            ICSharpFunctionDeclaration functionDeclaration, string parameterName)
        {
            return functionDeclaration.GetPreconditions()
                .Any(p => p.ChecksForNull(parameterName));
        }
    }
}