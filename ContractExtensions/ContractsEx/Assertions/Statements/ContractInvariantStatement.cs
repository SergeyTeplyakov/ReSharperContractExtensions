using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractInvariantStatement : ContractStatementBase
    {
        internal ContractInvariantStatement(ICSharpStatement statement, ContractInvariantExpression invariantExpression)
            : base(statement, invariantExpression)
        {
            InvariantExpression = invariantExpression;
        }

        public ContractInvariantExpression InvariantExpression { get; private set; }

        [CanBeNull]
        public static ContractInvariantStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invocationExpression = AsInvocationExpression(statement);

            var contractExpression = CodeContractExpression.FromInvocationExpression(invocationExpression);

            if (!(contractExpression is ContractInvariantExpression))
                return null;

            return new ContractInvariantStatement(statement, (ContractInvariantExpression)contractExpression);
        }
    }
}