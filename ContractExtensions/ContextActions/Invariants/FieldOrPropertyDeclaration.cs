using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Invariants
{
    internal struct FieldOrPropertyDeclaration
    {
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(IsField || IsProperty);
            Contract.Invariant(!Name.IsNullOrEmpty());
            Contract.Invariant(Type != null);
        }

        // TODO: Add Invariant that one of them is not null!
        public IFieldDeclaration Field { get; private set; }
        public IPropertyDeclaration Property { get; private set; }

        public bool IsField { get { return Field != null; } }
        public bool IsProperty { get { return Property != null; } }

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
}