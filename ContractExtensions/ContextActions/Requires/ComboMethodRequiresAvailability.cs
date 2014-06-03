using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Checks that combo actions that add requires for all method argument should be available.
    /// </summary>
    internal sealed class ComboMethodRequiresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly ICSharpFunctionDeclaration _selectedFunctionDeclaration;
        private readonly AddContractAvailability _addContractAvailability;
        private readonly List<string> _argumentNames;

        private ComboMethodRequiresAvailability()
        {}

        private ComboMethodRequiresAvailability(ICSharpContextActionDataProvider provider)
        {
            _provider = provider;

            if (IsAvailableFor(out _argumentNames, out _selectedFunctionDeclaration) &&
                CanAddContractClassIfNecessary(_selectedFunctionDeclaration, out _addContractAvailability))
            {
                IsAvailable = true;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || SelectedFunctionDeclaration != null);
            Contract.Invariant(!IsAvailable || ArgumentNames != null);
            Contract.Invariant(!IsAvailable || ArgumentNames.Count > 0,
                "If this action is available, we should have move than one available argument name!");
        }

        public static ComboMethodRequiresAvailability Create(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            Contract.Ensures(Contract.Result<ComboMethodRequiresAvailability>() != null);

            return new ComboMethodRequiresAvailability(provider);
        }

        public static readonly ComboMethodRequiresAvailability Unavailable = new ComboMethodRequiresAvailability {IsAvailable = false};
        public ICSharpFunctionDeclaration SelectedFunctionDeclaration { get { return _selectedFunctionDeclaration; } }
        [CanBeNull]
        public AddContractAvailability AddContractAvailability { get { return _addContractAvailability; } }
        public bool IsAvailable { get; private set; }
        public IList<string> ArgumentNames { get { return _argumentNames; } }

        [System.Diagnostics.Contracts.Pure]
        private bool IsAvailableFor(out List<string> availableArguments, 
            out ICSharpFunctionDeclaration selectedFunction)
        {
            availableArguments = new List<string>();
            selectedFunction = null;

            if (!MethodDeclarationSelected())
                return false;

            selectedFunction = GetSelectedFunctionDeclaration();

            if (selectedFunction == null)
                return false;

            availableArguments = GetArgumentsAvailableForRequiresCheck(selectedFunction);
            return availableArguments.Count != 0;
        }

        private bool CanAddContractClassIfNecessary(ICSharpFunctionDeclaration selectedFunction,
            out AddContractAvailability addContractAvailability)
        {
            // Adding contract class could be unavailable, thats ok, because this "combo" part is optional!
            addContractAvailability = AddContractAvailability.IsAvailableForSelectedMethod(_provider, selectedFunction);
            return true;
        }

        [System.Diagnostics.Contracts.Pure]
        private bool MethodDeclarationSelected()
        {
            // This combo is available only if constructor or method declaration is selected
            return _provider.IsSelected<IIdentifier>() &&
                   (_provider.IsSelected<IFunctionDeclaration>() ||
                    _provider.IsSelected<IConstructorDeclaration>());
        }

        [System.Diagnostics.Contracts.Pure]
        private ICSharpFunctionDeclaration GetSelectedFunctionDeclaration()
        {
            return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
        }

        [System.Diagnostics.Contracts.Pure]
        private List<string> GetArgumentsAvailableForRequiresCheck(ICSharpFunctionDeclaration selectedMethod)
        {
            Contract.Requires(selectedMethod != null);
            Contract.Ensures(Contract.Result<List<string>>() != null);

            var parameterRequiresAvailability =
                selectedMethod.DeclaredElement.Parameters
                    .SelectMany(pm => pm.GetDeclarations())
                    .Select(pm => pm as IParameterDeclaration)
                    .Where(pm => pm != null)
                    .Select(p => ParameterRequiresAvailability.Create(_provider, p))
                    .Where(p => p.IsAvailable)
                    .ToList();

            // Contract function could be absent, but in this case we'll generate contract
            // class (and function) before adding all preconditions!
            var contractFunction = selectedMethod.GetContractFunction();
            if (contractFunction == null)
            {
                return parameterRequiresAvailability.Select(p => p.ParameterName).ToList();
            }

            var availableArguments =
                parameterRequiresAvailability
                    .Select(pa => new FunctionRequiresAvailability(_provider,
                        pa.ParameterName, contractFunction))
                    .Where(fa => fa.IsAvailable)
                    .Select(fa => fa.ParameterName)
                    .ToList();

            return availableArguments;
        }


    }
}