using System;
using System.Diagnostics.Contracts;

namespace ReSharper.ContractExtensions.Utilities
{
    public static class Monadic
    {
        // TODO: consider to use debug version with Expression and with Wrapper, that will show, why its null!
        public static U With<T, U>(this T callSite, Func<T, U> selector) where T : class where U : class
        {
            Contract.Requires(selector != null);

            if (callSite == null)
                return default(U);

            return selector(callSite);
        }

        public static U Return<T, U>(this T callSite, Func<T, U> selector, U @default) where T : class where U : class
        {
            Contract.Requires(selector != null);

            if (callSite == null)
                return @default;

            return selector(callSite) ?? @default;
        }

        public static U Return<T, U>(this T callSite, Func<T, U> selector) where T : class where U : class
        {
            Contract.Requires(selector != null);

            if (callSite == null)
                return default(U);

            return selector(callSite);
        }

        public static void Do<T>(this T callSite, Action<T> action)
        {
            if (callSite != null)
                action(callSite);
        }

        public static U Do<T, U>(this T callSite, Func<T, U> func)
        {
            if (callSite != null)
                return func(callSite);
            return default(U);
        }

    }
}