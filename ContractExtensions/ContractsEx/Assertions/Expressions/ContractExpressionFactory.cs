using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx
{
    public static class ContractExpressionFactory
    {
        [CanBeNull]
        public static ContractExpressionBase FromInvocationExpression(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            return null;
        }
    }
}