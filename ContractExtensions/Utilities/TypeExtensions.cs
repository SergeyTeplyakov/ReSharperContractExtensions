using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class TypeExtensions
    {
        public static bool IsAttribute(this System.Type type)
        {
            Contract.Requires(type != null);

            return type.GetBaseTypes().Any(t => t == typeof (System.Attribute));
        }

        public static IEnumerable<System.Type> GetBaseTypes(this System.Type type)
        {
            Contract.Requires(type != null);

            return GetBaseTypesImpl(type);
        }

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