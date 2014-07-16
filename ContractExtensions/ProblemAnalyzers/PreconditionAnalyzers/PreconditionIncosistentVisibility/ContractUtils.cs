using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    internal static class ContractUtils
    {
        public static readonly Func<IParameter, bool> IsStringParameter =
            p => p.Type.GetClrTypeName().FullName == typeof(string).FullName;

        public static readonly Func<IConstructor, bool> HasOneStringArgument =
                c => c.Parameters.Count == 1 && IsStringParameter(c.Parameters[0]);

        public static readonly Func<IConstructor, bool> HasTwoStringArgument =
            c => c.Parameters.Count == 2 && IsStringParameter(c.Parameters[0]) && IsStringParameter(c.Parameters[1]);

        public static bool IsSutableForGenericContractRequires(this IConstructor constructor)
        {
            Contract.Requires(constructor != null);

            return HasOneStringArgument(constructor) || HasTwoStringArgument(constructor);
        }
    }
}