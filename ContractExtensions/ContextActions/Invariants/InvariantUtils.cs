using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Invariants
{
    internal static class InvariantUtils
    {
        public const string InvariantMethodName = "ObjectInvariant";

        /// <summary>
        /// Return "the only" invariant method with "ObjectInvariant" name.
        /// </summary>
        public static IMethodDeclaration GetInvariantMethod(this IClassLikeDeclaration classDeclaration)
        {
            Contract.Requires(classDeclaration != null);

            return classDeclaration.MethodDeclarations
                .FirstOrDefault(mi => mi.DeclaredName == InvariantUtils.InvariantMethodName);
        }

        /// <summary>
        /// Return all methods, marked with <see cref="ContractInvariantMethodAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Any class (or struct) could have any number of "invariant methods".
        /// </remarks>
        public static IEnumerable<IMethodDeclaration> GetInvariantMethods(this IClassLikeDeclaration classDeclaration)
        {
            Contract.Requires(classDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<IMethodDeclaration>>() != null);

            // Class could have any number of invariant methods!
            return classDeclaration.MethodDeclarations.Where(IsObjectInvariantMethod);
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