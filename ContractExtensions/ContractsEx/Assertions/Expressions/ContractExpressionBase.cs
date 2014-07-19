using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    /// <summary>
    /// Base class for every contract expressions like Contract.Requires, Contract.Ensures etc,
    /// and even for if-throw requires.
    /// </summary>
    public abstract class ContractExpressionBase
    {
        private readonly ContractExpressionType _expressionType;
        private readonly List<PredicateCheck> _predicates;
        private readonly Message _message;

        protected ContractExpressionBase(List<PredicateCheck> predicates, Message message)
        {
            Contract.Requires(predicates != null);
            Contract.Requires(message != null);

            _predicates = predicates;
            _message = message;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Predicates != null);
        }

        public ReadOnlyCollection<PredicateCheck> Predicates
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<PredicateCheck>>() != null);
                return _predicates.AsReadOnly();
            }
        }

        public bool CheckArgumentIsNull(Func<PredicateArgument, bool> comparer)
        {
            return _predicates.Any(pc => pc.ChecksForNull() && comparer(pc.Argument));
        }

        public bool CheckArgumentIsNotNull(Func<PredicateArgument, bool> comparer)
        {
            return _predicates.Any(pc => pc.ChecksForNotNull() && comparer(pc.Argument));
        }

        public Message Message
        {
            get
            {
                Contract.Ensures(Contract.Result<Message>() != null);
                return _message;
            }
        }

        protected static Message ExtractMessage(ICSharpArgument argument)
        {
            Contract.Requires(argument != null);
            Contract.Ensures(Contract.Result<Message>() != null);

            return argument.Expression
                .With(x => x as ICSharpLiteralExpression)
                .With(x => x.Literal.GetText())
                .Return(x => new LiteralMessage(x) as Message, NoMessage.Instance);
        }
    }
}