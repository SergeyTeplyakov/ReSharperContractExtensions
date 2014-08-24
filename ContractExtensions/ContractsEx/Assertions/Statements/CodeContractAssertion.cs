using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Represents one Assertion from Code Contract library, like Contract.Requires, Contract.Invariant etc.
    /// </summary>
    /// <remarks>
    /// Every valid precondtion contains following:
    /// Contract.Method(originalPredicates, message).
    /// 
    /// Note that this class is not suitable for Contract.Ensures because it has slightly 
    /// different internal structure.
    /// </remarks>
    public abstract class CodeContractAssertion : ContractStatement, ICodeContractAssertion
    {
        protected CodeContractAssertion(ICSharpStatement statement, 
            PredicateExpression predicateExpression, Message message)
            : base(statement, predicateExpression, message)
        {}

        public IExpression OriginalPredicateExpression
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpression>() != null);
                return _predicateExpression.OriginalPredicateExpression;
            }
        }

        public abstract ContractAssertionType AssertionType { get; }

        [CanBeNull]
        internal static CodeContractAssertion TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            return Impl.StatementUtils.AsInvocationExpression(statement).Return(FromInvocationExpression);
        }

        [CanBeNull]
        internal static CodeContractAssertion FromInvocationExpression(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            var statement = invocationExpression.GetContainingStatement();
            Contract.Assert(statement != null);

            ContractAssertionType? assertionType = GetContractAssertionType(invocationExpression);
            if (assertionType == null)
                return null;

            Contract.Assert(invocationExpression.Arguments.Count != 0, "Invocation expression should have at least one argument!");
            
            IExpression originalPredicateExpression = invocationExpression.Arguments[0].Expression;

            var predicateExpression = PredicateExpression.Create(originalPredicateExpression);
            var message = ExtractMessage(invocationExpression);

            // TODO: switch to dictionary of factory methods?
            switch (assertionType.Value)
            {
                case ContractAssertionType.Requires:
                    return new ContractRequires(statement, invocationExpression, 
                        predicateExpression, message);
                case ContractAssertionType.Ensures:
                    return new ContractEnsures(statement, predicateExpression, message);
                case ContractAssertionType.Invariant:
                    return new ContractInvariant(statement, predicateExpression, message);
                case ContractAssertionType.Assert:
                    return new ContractAssert(statement, predicateExpression, message);
                case ContractAssertionType.Assume:
                    return new ContractAssume(statement, predicateExpression, message);
                default:
                    Contract.Assert(false, "Unknown assertion type: " + assertionType.Value);
                    return null;
            }
        }

        private static ContractAssertionType? GetContractAssertionType(IInvocationExpression invocationExpression)
        {
            var clrType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();

            if (clrType.Return(x => x.FullName) != typeof(Contract).FullName)
                return null;

            return ParseAssertionType(method);
        }

        private static ContractAssertionType? ParseAssertionType(string method)
        {
            ContractAssertionType result;
            if (Enum.TryParse(method, out result))
                return result;
            return null;
        }

        private static Message ExtractMessage(IInvocationExpression invocationExpression)
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