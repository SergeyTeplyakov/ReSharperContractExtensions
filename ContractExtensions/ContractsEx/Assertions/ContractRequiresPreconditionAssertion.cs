using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    internal sealed class ContractRequiresPreconditionAssertion : ContractPreconditionAssertion
    {
        private readonly ContractAssertionExpression _contractAssertionExpression;

        private ContractRequiresPreconditionAssertion(ICSharpStatement statement, 
            ContractAssertionExpression contractAssertionExpression)
            : base(statement)
        {
            _contractAssertionExpression = contractAssertionExpression;
        }

        public override bool IsCodeContractBasedPrecondition
        {
            get { return true; }
        }

        public override bool ChecksForNull(string name)
        {
            return _contractAssertionExpression.PreconditionExpressions.Any(n => n.ChecksForNull(name));
        }

        public bool IsGeneric { get; private set; }

        [CanBeNull]
        new internal static ContractRequiresPreconditionAssertion TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invokedExpression = AsInvocationExpression(statement);
            if (invokedExpression == null)
                return null;

            var assertion = ContractAssertionExpression.FromInvocationExpression(invokedExpression);
            if (assertion != null && assertion.AssertionType == AssertionType.Precondition)
                return new ContractRequiresPreconditionAssertion(statement, assertion);

            return null;
        }
    }
}