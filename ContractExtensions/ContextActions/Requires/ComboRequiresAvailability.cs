using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
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
        private readonly IDeclaredType _parameterType;
        private readonly ICSharpFunctionDeclaration _selectedAbstractMethod;
        private readonly AddContractAvailability _addContractAvailability;

        private ComboRequiresAvailability()
        {}

        public ComboRequiresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;

            _selectedAbstractMethod = GetSelectedFunctionDeclaration();

            if (_selectedAbstractMethod != null
                && IsAbstractClassOrInterface() 
                && IsRequiresAvailableFor(out _parameterName, out _parameterType) 
                && CanAddContractForSelectedMethod(_selectedAbstractMethod, out _addContractAvailability))
            {
                IsAvailable = true;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _parameterName != null);
            Contract.Invariant(!IsAvailable || _parameterType != null);
            Contract.Invariant(!IsAvailable || _selectedAbstractMethod != null);
        }

        public bool IsAvailable { get; private set; }
        public string ParameterName { get { return _parameterName; } }
        public IDeclaredType ParameterType { get { return _parameterType; } }
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

        private bool CanAddContractForSelectedMethod(ICSharpFunctionDeclaration selectedFunction, 
            out AddContractAvailability addContractAvailability)
        {
            addContractAvailability = AddContractAvailability.IsAvailableForSelectedMethod(_provider, selectedFunction);
            return addContractAvailability.IsAvailable;
        }

        /// <summary>
        /// Return selected function declaration (method declaration or property declaration)
        /// </summary>
        /// <returns></returns>
        [Pure]
        private ICSharpFunctionDeclaration GetSelectedFunctionDeclaration()
        {
            var selectedMethod = _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
            if (selectedMethod != null)
                return selectedMethod;

            var propertyDeclaration = _provider.GetSelectedElement<IPropertyDeclaration>(true, true);
            if (propertyDeclaration == null || propertyDeclaration.IsAuto)
                return null;

            return propertyDeclaration.AccessorDeclarations.FirstOrDefault(a => a.Kind == AccessorKind.SETTER);
        }
      
        private bool IsRequiresAvailableFor(out string parameterName, out IDeclaredType parameterType)
        {
            parameterName = null;
            parameterType = null;

            var parameterRequiresAvailability = ParameterRequiresAvailability.Create(_provider);
            if (parameterRequiresAvailability.IsAvailable)
            {
                parameterName = parameterRequiresAvailability.ParameterName;
                parameterType = parameterRequiresAvailability.ParameterType;
                return true;
            }

            var propertySetterRequiresAvailability = new PropertySetterRequiresAvailability(_provider);
            if (propertySetterRequiresAvailability.IsAvailable)
            {
                parameterName = "value";
                parameterType = propertySetterRequiresAvailability.PropertyType;
                return true;
            }

            return false;
        }
    }
}