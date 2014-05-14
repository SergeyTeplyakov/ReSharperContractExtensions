using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve.Managed;
using JetBrains.ReSharper.Psi.Util;

namespace ReSharper.ContractExtensions.Utilities
{
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