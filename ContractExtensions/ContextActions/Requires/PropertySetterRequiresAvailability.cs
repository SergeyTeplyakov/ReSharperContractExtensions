using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.DataStructures;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    internal sealed class PropertySetterRequiresAvailability : ContextActionAvailabilityBase<PropertySetterRequiresAvailability>
    {
        private readonly ICSharpFunctionDeclaration _currentFunction;
        private readonly IClrTypeName _propertyType;

        private readonly bool _isIndexer;

        private readonly ICSharpFunctionDeclaration _getterIndexerDeclaration;
        private readonly ICSharpFunctionDeclaration _setterIndexerDeclaration;
        private readonly string _parameterName;

        public PropertySetterRequiresAvailability()
        {}

        public PropertySetterRequiresAvailability(ICSharpContextActionDataProvider provider)
            : base(provider)
        {
            _isAvailable = ComputeIsAvailable(out _currentFunction, out _propertyType);

            if (!_isAvailable)
            {
                _isAvailable = ComputeIsAvailableForIndexer(out _getterIndexerDeclaration, out _setterIndexerDeclaration,
                    out _parameterName, out _propertyType);
                _isIndexer = true;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            //Contract.Invariant(!IsAvailable || (!IsIndexer || _currentFunction != null),
            //    "For valid non-indexer properties _currentFunction should not be null");
            //Contract.Invariant(!IsAvailable || (IsIndexer || _getterIndexerDeclaration != null || _setterIndexerDeclaration != null),
            //    "For valid indexer setter or getter should be not null");
            Contract.Invariant(!IsAvailable || PropertyType != null);
        }

        public ReadOnlyCollection<ICSharpFunctionDeclaration> GetSelectedFunctions()
        {
            Contract.Ensures(Contract.Result<ReadOnlyCollection<ICSharpFunctionDeclaration>>() != null);
            Contract.Ensures(Contract.Result<ReadOnlyCollection<ICSharpFunctionDeclaration>>().Count != 0);

            return DoGetSelectedFunctions().ToList().AsReadOnly();
        }

        private IEnumerable<ICSharpFunctionDeclaration> DoGetSelectedFunctions()
        {
            if (IsIndexer)
            {
                if (_getterIndexerDeclaration != null)
                    yield return _getterIndexerDeclaration;
                if (_setterIndexerDeclaration != null)
                    yield return _setterIndexerDeclaration;
            }
            else
            {
                yield return _currentFunction;
            }
        }

        public ICSharpFunctionDeclaration SelectedFunctionDeclaration { get { return _currentFunction; } }
        public IClrTypeName PropertyType { get { return _propertyType; } }

        public bool IsIndexer { get { return _isIndexer; } }
        public ICSharpFunctionDeclaration IndexerGetterFunctionDeclaration { get { return _getterIndexerDeclaration; } }
        public ICSharpFunctionDeclaration IndexerSetterFunctionDeclaration { get { return _setterIndexerDeclaration; } }

        public string ParameterName
        {
            get { return _parameterName ?? "value"; }
        }

        private bool ComputeIsAvailableForIndexer(out ICSharpFunctionDeclaration getterDeclaration,
            out ICSharpFunctionDeclaration setterDeclaration, out string parameterName, 
            out IClrTypeName propertyType)
        {
            getterDeclaration = null;
            setterDeclaration = null;
            propertyType = null;
            parameterName = null;
            
            var indexerDeclaration = _provider.GetSelectedElement<IIndexerDeclaration>(true, true);
            if (indexerDeclaration == null)
                return false;

            getterDeclaration = indexerDeclaration.AccessorDeclarations.FirstOrDefault(a => a.Kind == AccessorKind.GETTER);
            setterDeclaration = indexerDeclaration.AccessorDeclarations.FirstOrDefault(a => a.Kind == AccessorKind.SETTER);
            if (getterDeclaration == null && setterDeclaration == null)
                return false;

            propertyType = indexerDeclaration.Type.GetClrTypeName();

            var accessorDeclaration = _provider.GetSelectedElement<IAccessorDeclaration>(true, true);
            if (accessorDeclaration != null)
            {
                parameterName = "value";
                // Selected property setter. Action should be available for reference and non-nullable value types.
                return indexerDeclaration.Type.IsReferenceOrNullableType();
            }

            var parameterDeclaration = ParameterRequiresAvailability.Create(_provider);
            parameterName = parameterDeclaration.ParameterName;
            return parameterDeclaration.IsAvailable;
        }

        private bool ComputeIsAvailable(out ICSharpFunctionDeclaration currentFunction,
            out IClrTypeName propertyType)
        {
            currentFunction = null;
            propertyType = null;

            var propertyDeclaration = _provider.GetSelectedElement<IPropertyDeclaration>(true, true);
            if (propertyDeclaration == null)
                return false;

            if (propertyDeclaration.IsAuto)
                return false;

            currentFunction = propertyDeclaration.AccessorDeclarations.FirstOrDefault(a => a.Kind == AccessorKind.SETTER);
            if (currentFunction == null)
                return false;

            propertyType = propertyDeclaration.Type.GetClrTypeName();

            return propertyDeclaration.Type.IsReferenceOrNullableType();
        }
    }
}