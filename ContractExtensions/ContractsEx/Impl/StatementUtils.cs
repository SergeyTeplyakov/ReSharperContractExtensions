using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Impl
{
    internal static class StatementUtils
    {
        public static IInvocationExpression AsInvocationExpression(ICSharpStatement statement)
        {
            var expressionStatement = statement as IExpressionStatement;
            if (expressionStatement == null)
                return null;

            var visitor = new InvocationVisitor();
            expressionStatement.Expression.Accept(visitor);
            if (visitor.InvocationExpression == null)
                return null;

            return visitor.InvocationExpression;
        }

    }
}