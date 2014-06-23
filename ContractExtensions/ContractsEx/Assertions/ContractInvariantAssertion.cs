using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractInvariantAssertion : ContractAssertion
    {
        private readonly ContractAssertionExpression _assertExpression;

        private ContractInvariantAssertion(ICSharpStatement statement, ContractAssertionExpression assertExpression) 
            : base(AssertionType.Invariant, statement, assertExpression.Message)
        {
            Contract.Requires(assertExpression != null);

            _assertExpression = assertExpression;
        }

        public override bool ChecksForNull(string name)
        {
            return _assertExpression.PreconditionExpressions.Any(p => p.ChecksForNotNull(name));
        }

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