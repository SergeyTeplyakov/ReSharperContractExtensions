using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Combo actions' availability that will check is it possible to add ContractClass attribute and
    /// then to add precondition.
    /// </summary>
    /// <remarks>
    /// Combo requires will try to add ContractClass attribute for the interface or abstract class and then
    /// will add Contract.Requires statement.
    /// </remarks>
    internal sealed class ComboRequiresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly string _parameterName;
        private readonly ICSharpFunctionDeclaration _selectedAbstractMethod;
        private readonly AddContractAvailability _addContractAvailability;

        private ComboRequiresAvailability()
        {}

        public ComboRequiresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;

            _selectedAbstractMethod = GetSelectedMethod();

            if (IsAbstractClassOrInterface() 
                && IsRequiresAvailableFor(out _parameterName) 
                && CanAddContractForSelectedMethod(out _addContractAvailability))
            {
                IsAvailable = true;
            }
        }

        private bool CanAddContractForSelectedMethod(out AddContractAvailability addContractAvailability)
        {
            addContractAvailability = AddContractAvailability.IsAvailableForSelectedMethod(_provider);
            return addContractAvailability.IsAvailable;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _parameterName != null);
            Contract.Invariant(!IsAvailable || _selectedAbstractMethod != null);
        }

        public bool IsAvailable { get; private set; }
        public string ParameterName { get { return _parameterName; } }
        public ICSharpFunctionDeclaration SelectedFunction { get { return _selectedAbstractMethod; } }
        public AddContractAvailability AddContractAvailability { get { return _addContractAvailability; } }

        public static readonly ComboRequiresAvailability Unavailable = new ComboRequiresAvailability { IsAvailable = false };

        private bool IsAbstractClassOrInterface()
        {
            if (_provider.IsSelected<IInterfaceDeclaration>())
                return true;

            var classDeclaration = _provider.GetSelectedElement<IClassDeclaration>(true, true);

            if (classDeclaration == null)
                return false; // disabling if outside the class declaration
            
            return classDeclaration.IsAbstract;
        }

        private ICSharpFunctionDeclaration GetSelectedMethod()
        {
            return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
        }

        private bool CanGenerateContractFor(ICSharpFunctionDeclaration selectedAbstractMethod)
        {
            // I don't know right now when I can't generate contract for abstract method or interface.
            // The only case: this function should exists!
            return selectedAbstractMethod != null;
        }
      
        private bool IsRequiresAvailableFor(out string parameterName)
        {
            parameterName = null;

            var parameterRequiresAvailability = ParameterRequiresAvailability.Create(_provider);
            if (parameterRequiresAvailability.IsAvailable)
            {
                parameterName = parameterRequiresAvailability.ParameterName;
                return true;
            }

            return false;
        }
    }
}