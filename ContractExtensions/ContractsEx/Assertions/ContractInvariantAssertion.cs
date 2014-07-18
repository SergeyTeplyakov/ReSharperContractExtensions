using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractInvariantAssertion : ContractAssertion
    {
        internal ContractInvariantAssertion(ICSharpStatement statement, ContractAssertionExpression assertExpression) 
            : base(AssertionType.Invariant, statement, assertExpression)
        {}

        [CanBeNull]
        public static ContractInvariantAssertion TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invocationExpression = AsInvocationExpression(statement);

            var assertion = ContractAssertionExpression.FromInvocationExpression(invocationExpression);

            if (assertion == null || assertion.AssertionType != AssertionType.Invariant)
                return null;

            return new ContractInvariantAssertion(statement, assertion);
        }
    }
}