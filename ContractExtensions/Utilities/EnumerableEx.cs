using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class EnumerableEx
    {
        public static int LastIndexOf<T>(this IList<T> list, Func<T, bool> predicate)
        {
            Contract.Requires(list != null);
            Contract.Requires(predicate != null);

            for (int n = list.Count - 1; n >= 0; n--)
            {
                if (predicate(list[n]))
                {
                    return n;
                }
            }

            return -1;
        }

        public static int FirstIndexOf<T>(this IList<T> list, Func<T, bool> predicate)
        {
            Contract.Requires(list != null);
            Contract.Requires(predicate != null);

            for (int n = 0; n < list.Count; n++)
            {
                if (predicate(list[n]))
                    return n;
            }

            return -1;
        }
    }
}