using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.Utilities
{
    [Obsolete("Do not use IAttribute, get attribute instances instead via DeclaredElement property")]
    internal static class AttributeExtensions
    {
        [CanBeNull]
        public static IClrTypeName GetClrTypeName(this IAttribute attribute)
        {
            Contract.Requires(attribute != null);

            // TODO: is there any easier way to get ClrName??
            return attribute.TypeReference
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement)
                .With(x => x as ITypeElement)
                .Return(x => x.GetClrName());
        }
    }
}