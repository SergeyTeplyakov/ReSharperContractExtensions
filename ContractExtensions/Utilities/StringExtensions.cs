using System.Diagnostics.Contracts;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class StringExtensions
    {
        [Pure]
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }
    }
}