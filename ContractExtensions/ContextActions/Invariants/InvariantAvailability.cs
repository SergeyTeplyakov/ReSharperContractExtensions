using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Preconditions.Logic;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Invariants
{
    internal struct FieldOrPropertyDeclaration
    {
        // TODO: Add Invariant that one of them is not null!
        public IFieldDeclaration Field { get; private set; }
        public IPropertyDeclaration Property { get; private set; }

        public bool IsField { get { return Field != null; } }
        public bool IsProperty { get { return Property != null; } }

        // TODO: add invariant that Name and Type are not null
        public bool IsStatic { get; private set; }
        public string Name { get; private set; }
        public IType Type { get; private set; }

        public static FieldOrPropertyDeclaration FromFieldDeclaration(IFieldDeclaration fieldDeclaration)
        {
            Contract.Requires(fieldDeclaration != null);
            Contract.Requires(fieldDeclaration.NameIdentifier != null);
            Contract.Requires(fieldDeclaration.DeclaredElement != null);

            return new FieldOrPropertyDeclaration
            {
                IsStatic = fieldDeclaration.IsStatic,
                Name = fieldDeclaration.NameIdentifier.Name,
                Type = fieldDeclaration.DeclaredElement.Type,
                Field = fieldDeclaration,
            };
        }

        public static FieldOrPropertyDeclaration FromPropertyDeclaration(IPropertyDeclaration propertyDeclaration)
        {
            Contract.Requires(propertyDeclaration != null);

            return new FieldOrPropertyDeclaration
            {
                IsStatic = propertyDeclaration.IsStatic,
                Name = propertyDeclaration.NameIdentifier.Name,
                Type = propertyDeclaration.DeclaredElement.Type,
                Property = propertyDeclaration,
            };
        }
    }

    /// <summary>
    /// Represents object that shows whether Add Invariant action enabled or not.
    /// </summary>
    internal sealed class InvariantAvailability
    {
        private readonly FieldOrPropertyDeclaration _selectedElement;
        // This could be class or struct declarations!
        private readonly IClassLikeDeclaration _classDeclaration;
        public static readonly InvariantAvailability InvariantUnavailable = new InvariantAvailability {IsAvailable = false};

        private InvariantAvailability()
        {}

        private InvariantAvailability(ICSharpContextActionDataProvider provider,
            FieldOrPropertyDeclaration selectedElement)
        {
            Contract.Requires(provider != null);
            
            _classDeclaration = provider.GetSelectedElement<IClassLikeDeclaration>(true, true);
            _selectedElement = selectedElement;

            IsAvailable = AnalyzeAvailability();
            if (IsAvailable)
                SelectedMemberName = _selectedElement.Name;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(IsAvailable == false || !SelectedMemberName.IsNullOrEmpty(),
                "For available action, selected member should not be null");
            Contract.Invariant(IsAvailable == false || _classDeclaration != null);
        }

        public static InvariantAvailability Create(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            Contract.Ensures(Contract.Result<InvariantAvailability>() != null);

            var fieldOrPropertyDeclaration = TryCreateFieldOrProperty(provider);

            if (fieldOrPropertyDeclaration == null)
                return InvariantUnavailable;

            return new InvariantAvailability(provider, fieldOrPropertyDeclaration.Value);
        }

        private static FieldOrPropertyDeclaration? TryCreateFieldOrProperty(ICSharpContextActionDataProvider provider)
        {
            var fieldDeclaration = provider.GetSelectedElement<IFieldDeclaration>(true, true);
            if (fieldDeclaration != null && IsFieldDeclarationValid(fieldDeclaration))
                return FieldOrPropertyDeclaration.FromFieldDeclaration(fieldDeclaration);

            var propertyDeclaration = provider.GetSelectedElement<IPropertyDeclaration>(true, true);
            if (propertyDeclaration != null && IsPropertyDeclarationValid(propertyDeclaration))
                return FieldOrPropertyDeclaration.FromPropertyDeclaration(propertyDeclaration);

            return null;
        }

        private static bool IsPropertyDeclarationValid(IPropertyDeclaration propertyDeclaration)
        {
            // TODO: when property is valid?
            return true;
        }

        private static bool IsFieldDeclarationValid(IFieldDeclaration fieldDeclaration)
        {
            // TODO: when field is valid?
            return true;
        }

        private bool AnalyzeAvailability()
        {
            // Invariants are impossible on static fields
            if (_selectedElement.IsStatic)
                return false;

            if (!_selectedElement.Type.IsReferenceOrNullableType())
                return false;

            if (AlreadyPartOfObjectInvariant(_selectedElement.Name))
                return false;

            if (_selectedElement.IsProperty && !PropertyIsReadable())
                return false;

            return true;
        }

        private bool PropertyIsReadable()
        {
            Contract.Requires(_selectedElement.IsProperty);

            return _selectedElement.Property.AccessorDeclarations.Any(a => a.Kind == AccessorKind.GETTER);
        }

        private bool AlreadyPartOfObjectInvariant(string selectedName)
        {
            var invariantMethod = _classDeclaration
                .GetInvariantMethod()
                .Return(x => x.IsObjectInvariantMethod() ? x : null);

            if (invariantMethod == null)
                return false;

            return invariantMethod.GetInvariants().Any(s => s.ArgumentName == selectedName);
        }

        public bool IsAvailable { get; private set; }
        public string SelectedMemberName { get; private set; }
    }
}