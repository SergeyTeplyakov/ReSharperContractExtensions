using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ReSharper.ContractExtensions.Utilities
{
    public static class TypeExtensions
    {
        [Pure]
        public static bool IsAttribute(this System.Type type)
        {
            Contract.Requires(type != null);

            return type.GetBaseTypes().Any(t => t == typeof (System.Attribute));
        }

        [Pure]
        public static IEnumerable<System.Type> GetBaseTypes(this System.Type type)
        {
            Contract.Requires(type != null);

            return GetBaseTypesImpl(type);
        }

        [Pure]
        private static IEnumerable<System.Type> GetBaseTypesImpl(System.Type type)
        {
            System.Type currentType = type;
            while (currentType != typeof (object))
            {
                currentType = currentType.BaseType;
                yield return currentType;
            }
        }
    }
}