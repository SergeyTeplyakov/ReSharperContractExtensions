using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
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
        /// Functions, where the tool should insert precondition. Note that this could be different
        /// from the selected function (because selected function could be in the abstract class).
        /// There is a list of possible functions to insert precondition, because indexer could have
        /// more then one function to insert precondition: getter and setter.
        /// </summary>
        private readonly List<ICSharpFunctionDeclaration> _functionsToInsertPrecondition;

        public FunctionRequiresAvailability() { }

        public FunctionRequiresAvailability(ICSharpContextActionDataProvider provider, string parameterName, 
            IEnumerable<ICSharpFunctionDeclaration> selectedFunctions)
            : base(provider)
        {
            Contract.Requires(provider != null);
            Contract.Requires(!string.IsNullOrEmpty(parameterName));

            ParameterName = parameterName;

            _functionsToInsertPrecondition = 
                selectedFunctions
                .Select(GetContractFunction)
                .Where(x => x != null && IsRequiresAvailable(parameterName, x))
                .ToList();

            _isAvailable = _functionsToInsertPrecondition.Any();
        }

        public FunctionRequiresAvailability(ICSharpContextActionDataProvider provider, string parameterName, 
            ICSharpFunctionDeclaration selectedFunction = null)
            : this(provider, parameterName, new []{selectedFunction ?? GetSelectedFunctionDeclaration(provider)})
        {}

        [CanBeNull]
        private ICSharpFunctionDeclaration GetContractFunction(ICSharpFunctionDeclaration selectedFunction)
        {
            return (selectedFunction ?? GetSelectedFunctionDeclaration(_provider)).With(x => x.GetContractFunction());
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _functionsToInsertPrecondition.Count != 0);
            Contract.Invariant(!IsAvailable || ParameterName != null);
        }

        public string ParameterName { get; private set; }

        public ReadOnlyCollection<ICSharpFunctionDeclaration> FunctionsToInsertPrecondition
        {
            get { return _functionsToInsertPrecondition.AsReadOnly(); }
        }

        public ICSharpFunctionDeclaration FunctionToInsertPrecondition { get { return _functionsToInsertPrecondition[0]; } }

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

        private static ICSharpFunctionDeclaration GetSelectedFunctionDeclaration(ICSharpContextActionDataProvider provider)
        {
            return provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
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