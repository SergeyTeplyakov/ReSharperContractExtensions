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
    public abstract class ContractAssertionExpressionBase
    {
        private readonly AssertionType _assertionType;
        private readonly List<PredicateCheck> _predicates;
        private readonly Message _message;

        protected ContractAssertionExpressionBase(AssertionType assertionType, 
            List<PredicateCheck> predicates, Message message)
        {
            Contract.Requires(Enum.IsDefined(typeof (AssertionType), assertionType));
            Contract.Requires(predicates != null);
            Contract.Requires(message != null);

            _assertionType = assertionType;
            _predicates = predicates;
            _message = message;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Predicates != null);
        }

        public AssertionType AssertionType
        {
            get { return _assertionType; }
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

        protected static AssertionType? GetAssertionType(IInvocationExpression invocationExpression)
        {
            var clrType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();

            if (clrType.Return(x => x.FullName) != typeof (Contract).FullName)
                return null;

            return ParseAssertionType(method);
        }

        private static AssertionType? ParseAssertionType(string method)
        {
            switch (method)
            {
                case "Requires":
                    return AssertionType.Precondition;
                case "Ensures":
                    return AssertionType.Postcondition;
                case "Assert":
                    return AssertionType.Assertion;
                case "Invariant":
                    return AssertionType.Invariant;
                case "Assume":
                    return AssertionType.Assumption;
                default:
                    return null;
            }
        }

        protected static Message ExtractMessage(ICSharpArgument argument)
        {
            Contract.Requires(argument != null);
            Contract.Ensures(Contract.Result<Message>() != null);

            return argument.Expression
                .With(x => x as ICSharpLiteralExpression)
                .With(x => x.Literal.GetText())
                .Return(x => new StringMessage(x) as Message, NoMessage.Instance);
        }

        protected static Message ExtractMessage(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);
            Contract.Ensures(Contract.Result<Message>() != null);
            Contract.Assert(invocationExpression.Arguments.Count != 0);

            return invocationExpression.Arguments.Skip(1).FirstOrDefault()
                .With(x => x.Expression)
                .Return(Message.Create, NoMessage.Instance);
        }
    }
}