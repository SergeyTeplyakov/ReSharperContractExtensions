using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Impl;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Base class that represent any Code Contract Assertion like Contract.Requires, Contract.Ensures, Contract.Invariant etc.
    /// </summary>
    public abstract class ContractAssertion
    {
        protected ContractAssertion(AssertionType assertionType,
            ICSharpStatement statement, string message)
        {
            Contract.Requires(statement != null);

            Statement = statement;
            AssertionType = assertionType;
            Message = message;
        }

        public AssertionType AssertionType { get; private set; }
        public ICSharpStatement Statement { get; private set; }

        [CanBeNull]
        public string Message { get; private set; }

        /// <summary>
        /// Returns true if current Assertion checks for null something with specified <paramref name="name"/>.
        /// </summary>
        public abstract bool ChecksForNull(string name);

        public override string ToString()
        {
            return string.Format("Assertion type: {0}\nC# statement:\n{1}",
                AssertionType, Statement);
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