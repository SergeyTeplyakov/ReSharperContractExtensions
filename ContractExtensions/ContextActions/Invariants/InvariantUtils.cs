using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Invariants
{
    internal static class InvariantUtils
    {
        public const string InvariantMethodName = "ObjectInvariant";

        public static IMethodDeclaration GetInvariantMethod(this IClassLikeDeclaration classDeclaration)
        {
            Contract.Requires(classDeclaration != null);

            return classDeclaration.MethodDeclarations
                .FirstOrDefault(mi => mi.DeclaredName == InvariantUtils.InvariantMethodName);
        }

        public static bool IsObjectInvariantMethod(this IMethodDeclaration methodDeclaration)
        {
            return methodDeclaration.Attributes
                .Select(a => a.GetClrTypeName())
                .Where(a => a != null)
                .Any(a => a.FullName == typeof (ContractInvariantMethodAttribute).FullName);
        }

    }
}