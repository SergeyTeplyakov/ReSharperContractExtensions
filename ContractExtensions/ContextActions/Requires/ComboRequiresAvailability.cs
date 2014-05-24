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
        private readonly AddContractForAvailability _addContractForAvailability;

        private ComboRequiresAvailability()
        {}

        public ComboRequiresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;

            if (IsAbstractClassOrInterface() 
                && IsRequiresAvailableFor(out _parameterName) 
                && (IsContractClassGenerationAvailable(out _addContractForAvailability)))
            {
                IsAvailable = true;
            }
        }

        private bool IsAbstractClassOrInterface()
        {
            if (_provider.IsSelected<IInterfaceDeclaration>())
                return true;

            var classDeclaration = _provider.GetSelectedElement<IClassDeclaration>(true, true);

            if (classDeclaration == null)
                return false; // disabling if outside the class declaration
            
            return classDeclaration.IsAbstract;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _parameterName != null);
            Contract.Invariant(!IsAvailable || _addContractForAvailability != null);
        }

        public bool IsAvailable { get; private set; }
        public string ParameterName { get { return _parameterName; } }
        public AddContractForAvailability AddContractAvailability { get { return _addContractForAvailability; } }


        public static readonly ComboRequiresAvailability Unavailable = new ComboRequiresAvailability {IsAvailable = false};

        private bool IsContractClassGenerationAvailable(out AddContractForAvailability availability)
        {
            availability = new AddContractForAvailability(_provider, true);
            return availability.IsAvailable;
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