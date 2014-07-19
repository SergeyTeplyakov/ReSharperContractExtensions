using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Impl;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    // TODO: the whole hierarchy of contract statements seems unnecessary! Maybe expressions should be enough!!
    /// <summary>
    /// Base class that represents any contract statement including Code Contract
    /// statements like Contract.Requires/Ensures, as well as other form of contracts, like
    /// if-throw contracts and gurad-based contracts.
    /// </summary>
    public abstract class ContractStatementBase : IContractStatement
    {
        private readonly ICSharpStatement _statement;
        protected readonly ContractExpressionBase _expression;

        protected ContractStatementBase(ICSharpStatement statement, ContractExpressionBase expression)
        {
            Contract.Requires(statement != null);
            Contract.Requires(expression != null);

            _statement = statement;
            _expression = expression;
        }

        public ContractExpressionBase Expression
        {
            get
            {
                Contract.Ensures(Contract.Result<ContractExpressionBase>() != null);
                return _expression;
            }
        }

        public ICSharpStatement Statement
        {
            get
            {
                Contract.Ensures(Contract.Result<ICSharpStatement>() != null);
                return _statement;
            }
        }

        public Message Message
        {
            get
            {
                Contract.Ensures(Contract.Result<Message>() != null);
                return _expression.Message;
            }
        }

        /// <summary>
        /// Returns true if current Assertion checks for null something with specified <paramref name="name"/>.
        /// </summary>
        public virtual bool AssertsArgumentIsNotNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            return _expression.Predicates.Any(p => p.ChecksForNotNull(name));
        }

        public virtual bool AssertsArgumentIsNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            return _expression.Predicates.Any(p => p.ChecksForNull(name));
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
            return _expression.CheckArgumentIsNotNull(comparer);
        }

        public override string ToString()
        {
            return string.Format("'C# statement:\n{0}", Statement);
        }

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