using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    internal struct ArgumentDescription
    {
        public string Name;
        public IClrTypeName Type;
    }

    /// <summary>
    /// Checks that combo actions that add requires for all method argument should be available.
    /// </summary>
    internal sealed class ComboMethodRequiresAvailability : ContextActionAvailabilityBase<ComboMethodRequiresAvailability>
    {
        private readonly ICSharpFunctionDeclaration _selectedFunctionDeclaration;
        private readonly AddContractClassAvailability _addContractAvailability;
        private readonly List<ArgumentDescription> _argumentNames;

        public ComboMethodRequiresAvailability()
        {}

        public ComboMethodRequiresAvailability(ICSharpContextActionDataProvider provider)
            : base(provider)
        {

            if (IsAvailableFor(out _argumentNames, out _selectedFunctionDeclaration) &&
                CanAddContractClassIfNecessary(_selectedFunctionDeclaration, out _addContractAvailability))
            {
                _isAvailable = true;
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

        protected override void CheckAvailability()
        {}

        public static ComboMethodRequiresAvailability Create(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            Contract.Ensures(Contract.Result<ComboMethodRequiresAvailability>() != null);

            return new ComboMethodRequiresAvailability(provider);
        }

        public ICSharpFunctionDeclaration SelectedFunctionDeclaration { get { return _selectedFunctionDeclaration; } }

        [CanBeNull]
        public AddContractClassAvailability AddContractAvailability { get { return _addContractAvailability; } }
        public IList<ArgumentDescription> ArgumentNames { get { return _argumentNames; } }

        [System.Diagnostics.Contracts.Pure]
        private bool IsAvailableFor(out List<ArgumentDescription> availableArguments, 
            out ICSharpFunctionDeclaration selectedFunction)
        {
            availableArguments = new List<ArgumentDescription>();
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
            out AddContractClassAvailability addContractAvailability)
        {
            // Adding contract class could be unavailable, thats ok, because this "combo" part is optional!
            addContractAvailability = AddContractClassAvailability.IsAvailableForSelectedMethod(_provider, selectedFunction);
            return true;
        }

        [System.Diagnostics.Contracts.Pure]
        private bool MethodDeclarationSelected()
        {
            if (_provider.IsSelected<IParameterDeclaration>())
                return false;

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
        private List<ArgumentDescription> GetArgumentsAvailableForRequiresCheck(ICSharpFunctionDeclaration selectedMethod)
        {
            Contract.Requires(selectedMethod != null);
            Contract.Ensures(Contract.Result<List<ArgumentDescription>>() != null);

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
                return parameterRequiresAvailability.Select(p => 
                    new ArgumentDescription {Name = p.ParameterName, Type = p.ParameterType})
                    .ToList();
            }

            Func<ParameterRequiresAvailability, bool> isFuncAvailable =
                p => new FunctionRequiresAvailability(_provider, p.ParameterName, contractFunction).IsAvailable;

            var availableArguments =
                parameterRequiresAvailability
                    .Where(isFuncAvailable)
                    .Select(pa => 
                        new ArgumentDescription {Name = pa.ParameterName, Type = pa.ParameterType})
                    .ToList();

            return availableArguments;
        }


    }
}