using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Base class for every contract statements like Contract.Requires, Contract.Ensures etc,
    /// if-throw precondition, guard-base preconditions etc.
    /// </summary>
    public abstract class ContractStatement
    {
        private readonly ICSharpStatement _statement;
        protected readonly PredicateExpression _predicateExpression;
        protected readonly Message _message;

        protected ContractStatement(ICSharpStatement statement, PredicateExpression predicateExpression, Message message)
        {
            Contract.Requires(statement != null);
            Contract.Requires(predicateExpression != null);
            Contract.Requires(message != null);

            _statement = statement;
            _predicateExpression = predicateExpression;
            _message = message;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Predicates != null);
        }

        public ICSharpStatement CSharpStatement { get { return _statement; } }

        public ReadOnlyCollection<PredicateCheck> Predicates
        {
            get { return _predicateExpression.Predicates; }
        }

        /// <summary>
        /// Returns true if current Assertion checks for null something with specified <paramref name="name"/>.
        /// </summary>
        public virtual bool ChecksForNotNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            return Predicates.Any(p => p.ChecksForNotNull(name));
        }


        public bool ChecksForNotNull(Func<PredicateArgument, bool> comparer)
        {
            return Predicates.Any(pc => pc.ChecksForNotNull() && comparer(pc.Argument));
        }

        public Message Message
        {
            get { return _message; }
        }

        protected static Message ExtractMessage(ICSharpArgument argument)
        {
            Contract.Requires(argument != null);
            Contract.Ensures(Contract.Result<Message>() != null);

            return argument.Expression.With(Message.Create);
        }
    }
}