using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;

using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class MethodDeclarationEx
    {
        public static IDeclaredType GetReturnType(this IFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Requires(functionDeclaration.DeclaredElement != null);

            // TODO: add another precondition taht functionDeclaration is resolved!.
            // Only in this case we can ensure that result would be not null
            // Contract.Requires(functionDeclaration.Resolved().IsOk());

            Contract.Ensures(Contract.Result<IDeclaredType>() != null);

            return functionDeclaration.DeclaredElement.ReturnType as IDeclaredType;
        }
    }
}