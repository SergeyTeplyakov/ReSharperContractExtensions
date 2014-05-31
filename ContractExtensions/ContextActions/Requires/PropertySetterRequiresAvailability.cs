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

        public PropertySetterRequiresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;

            IsAvailable = ComputeIsAvailable(out _currentFunction);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _currentFunction != null);
        }

        public bool IsAvailable { get; private set; }
        public ICSharpFunctionDeclaration SelectedFunctionDeclaration { get { return _currentFunction; } }

        private bool ComputeIsAvailable(out ICSharpFunctionDeclaration currentFunction)
        {
            currentFunction = null;

            var propertyDeclaration = _provider.GetSelectedElement<IPropertyDeclaration>(true, true);
            if (propertyDeclaration == null)
                return false;

            if (propertyDeclaration.IsAuto)
                return false;

            currentFunction = propertyDeclaration.AccessorDeclarations.FirstOrDefault(a => a.Kind == AccessorKind.SETTER);
            if (currentFunction == null)
                return false;

            return propertyDeclaration.Type.IsReferenceOrNullableType();
        }
    }
}