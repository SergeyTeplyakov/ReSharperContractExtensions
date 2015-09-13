using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    internal sealed class ComboEnsuresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly AddContractClassAvailability _addContractAvailability;
        private readonly ICSharpFunctionDeclaration _selectedFunctionDeclaration;
        private ComboEnsuresAvailability()
        {}

        public ComboEnsuresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            _provider = provider;

            if (IsAbstractClassOrInterface()
                && IsEnsuresAvailableFor(out _selectedFunctionDeclaration)
                && CanAddContractForSelectedMethod(out _addContractAvailability,
                        _selectedFunctionDeclaration))
            {
                IsAvailable = true;
            }
        }

        public bool IsAvailable { get; private set; }
        public AddContractClassAvailability AddContractAvailability { get { return _addContractAvailability; } }
        public ICSharpFunctionDeclaration SelectedFunction { get { return _selectedFunctionDeclaration; } }

        public static readonly ComboEnsuresAvailability Unavailable = new ComboEnsuresAvailability {IsAvailable = false};

        private bool IsAbstractClassOrInterface()
        {
            if (_provider.IsSelected<IInterfaceDeclaration>())
                return true;

            var classDeclaration = _provider.GetSelectedElement<IClassDeclaration>(true, true);

            if (classDeclaration == null)
                return false; // disabling if outside the class declaration

            return classDeclaration.IsAbstract;
        }

        private bool IsEnsuresAvailableFor(out ICSharpFunctionDeclaration selectedFunctionDeclaration)
        {
            var returnTypeEnsuresAvailability = new NullCheckReturnTypeEnsuresAvailability(_provider);
            selectedFunctionDeclaration = returnTypeEnsuresAvailability.SelectedFunctionDeclaration;
            return returnTypeEnsuresAvailability.IsAvailable;
        }

        private bool CanAddContractForSelectedMethod(out AddContractClassAvailability addContractAvailability, 
            ICSharpFunctionDeclaration selectedFunction)
        {
            addContractAvailability = AddContractClassAvailability.IsAvailableForSelectedMethod(_provider, selectedFunction);
            return addContractAvailability.IsAvailable;
        }
    }
}