using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Util;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class JetTypeEx
    {
        public static bool IsReferenceOrNullableType(this IType type)
        {
            Contract.Requires(type != null);

            return type.IsReferenceType() || type.IsNullable();
        }

        public static IClrTypeName GetClrTypeName(this IType type)
        {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<IClrTypeName>() != null);

            var clrTypeName =  
                type.With(x => x as IDeclaredType)
                .Return(x => x.GetClrName());

            if (clrTypeName != null)
                return clrTypeName;

            return new ClrTypeName(type.GetLongPresentableName(CSharpLanguage.Instance));
        }
    }
}