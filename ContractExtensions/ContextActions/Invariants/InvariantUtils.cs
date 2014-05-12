using System.Diagnostics.Contracts;
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
            // TODO: this definetely more correct way (because it will work only for resolved types)
            // but not sure about performance and simplicity!
            return methodDeclaration.Attributes.Any(
                n => n.With(x => x.TypeReference)
                      .With(x => x.Resolve())
                      .With(x => x.DeclaredElement)
                      .Return(x => x.ShortName) == typeof (ContractInvariantMethodAttribute).Name);
        }

    }
}