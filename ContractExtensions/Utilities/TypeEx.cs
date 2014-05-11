using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class TypeEx
    {
        public static bool IsReferenceOrNullableType(this IType type)
        {
            Contract.Requires(type != null);

            return type.IsReferenceType() || type.IsNullable();
        }
    }
}