using System;
using System.Diagnostics.Contracts;
using System.Linq;
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
        protected readonly ContractAssertionExpressionBase _assertionExpression;
        private Message _message;

        protected ContractAssertion(AssertionType assertionType,
            ICSharpStatement statement, ContractAssertionExpressionBase assertionExpression)
        {
            Contract.Requires(statement != null);
            Contract.Requires(assertionExpression != null);

            _assertionExpression = assertionExpression;

            Statement = statement;
            AssertionType = assertionType;
            _message = assertionExpression.Message;
        }

        public AssertionType AssertionType { get; private set; }
        public ICSharpStatement Statement { get; private set; }

        public Message Message
        {
            get
            {
                Contract.Ensures(Contract.Result<Message>() != null);
                return _message;
            }
        }

        /// <summary>
        /// Returns true if current Assertion checks for null something with specified <paramref name="name"/>.
        /// </summary>
        public virtual bool AssertsArgumentIsNotNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            return _assertionExpression.Predicates.Any(p => p.ChecksForNotNull(name));
        }

        public virtual bool AssertsArgumentIsNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            return _assertionExpression.Predicates.Any(p => p.ChecksForNull(name));
        }

        /// <summary>
        /// Checks whether assertion is a null check based on the specified <paramref name="comparer"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="PredicateCheck"/> contains <see cref="PredicateArgument"/> as an argument that could have more
        /// complex behavior that simple string-to-string comparison.
        /// For instance, in some cases we should now whether assertion checks for specified argument directly
        /// (like Contract.Requires(person != null);), or it checks some property of the argument 
        /// (like Contract.Requires(person.Name != null);). In some cases we need first check, but in some other
        /// cases we need a second check.
        /// </remarks>
        public virtual bool AssertsArgumentIsNotNull(Func<PredicateArgument, bool> comparer)
        {
            Contract.Requires(comparer != null);
            return _assertionExpression.CheckArgumentIsNotNull(comparer);
        }

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