using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using System;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class MethodDeclarationEx
    {
        [System.Diagnostics.Contracts.Pure]
        [CanBeNull]
        public static IDeclaredType GetReturnType(this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Requires(functionDeclaration.IsValidForContracts());

            // TODO: add another precondition taht functionDeclaration is resolved!.
            // Only in this case we can ensure that result would be not null
            // Contract.Requires(functionDeclaration.Resolved().IsOk());

            return functionDeclaration.DeclaredElement.ReturnType as IDeclaredType;
        }

        [System.Diagnostics.Contracts.Pure]
        public static bool HasAttribute(this ICSharpFunctionDeclaration functionDeclaration, Type attributeType)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Requires(functionDeclaration.IsValidForContracts());
            Contract.Requires(attributeType != null);
            Contract.Requires(attributeType.IsAttribute());

            return functionDeclaration.DeclaredElement.HasAttributeInstance(
                new ClrTypeName(attributeType.FullName), inherit: true);
        }

        [System.Diagnostics.Contracts.Pure]
        public static bool IsValidForContracts(this ICSharpFunctionDeclaration functionDeclaration)
        {
            // TODO: is there any other criterias??

            return functionDeclaration != null && 
                   functionDeclaration.DeclaredElement != null &&
                   // TODO: removed this, because for abstract method we're considering only return types
                   // but not the body!
                   //functionDeclaration.Body != null &&
                   functionDeclaration.IsValid() &&
                   functionDeclaration.GetReturnType() != null;
        }
    }
}