using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class ClassLikeDeclarationExtensions
    {
        public static bool HasAttribute(this IClassLikeDeclaration classLikeDeclaration, Type attributeType)
        {
            Contract.Requires(classLikeDeclaration != null);
            Contract.Requires(classLikeDeclaration.DeclaredElement != null);
            Contract.Requires(attributeType != null);
            Contract.Requires(attributeType.IsAttribute());

            var clrAttribute = new ClrTypeName(attributeType.FullName);
            return classLikeDeclaration.DeclaredElement.HasAttributeInstance(
                new ClrTypeName(attributeType.FullName), inherit: true);
        }
    }
}