using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Refactorings.Util;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class ClassEx
    {
        public static bool IsSuperType(this IClass @class, IClass superClass)
        {
            Contract.Requires(@class != null);
            Contract.Requires(superClass != null);

            bool isSuperType = false;
            SuperTypesUtil.ProcessClassAndSuperClasses(@class,
                bc =>
                    {
                        if (bc.Equals(superClass))
                            isSuperType = true;
                    });
            return isSuperType;
        }
    }
}