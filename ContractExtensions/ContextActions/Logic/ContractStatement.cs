using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.Preconditions.Logic
{
    /// <summary>
    /// Root class for such contract statements as Precondition, Postcondition or
    /// object invariant.
    /// </summary>
    internal abstract class ContractStatement
    {
        protected ContractStatement(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            Statement = statement;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Statement != null);
        }

        public ICSharpStatement Statement { get; private set; }

        public override sealed string ToString()
        {
            return Statement.GetText();
        }

        protected static IInvocationExpression AsInvocationExpression(ICSharpStatement statement)
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