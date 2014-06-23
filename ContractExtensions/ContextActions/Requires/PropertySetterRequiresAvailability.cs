using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    internal sealed class PropertySetterRequiresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly ICSharpFunctionDeclaration _currentFunction;
        private readonly IClrTypeName _propertyType;

        public PropertySetterRequiresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;

            IsAvailable = ComputeIsAvailable(out _currentFunction, out _propertyType);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _currentFunction != null);
            Contract.Invariant(!IsAvailable || PropertyType != null);
        }

        public bool IsAvailable { get; private set; }
        public ICSharpFunctionDeclaration SelectedFunctionDeclaration { get { return _currentFunction; } }
        public IClrTypeName PropertyType { get { return _propertyType; } }

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