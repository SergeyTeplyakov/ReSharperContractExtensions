using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractEnsuresAssertion : ContractAssertion
    {
        private readonly ContractEnsureExpression _contractEnsure;

        private ContractEnsuresAssertion(ICSharpStatement statement, ContractEnsureExpression contractEnsure) 
            : base(AssertionType.Precondition, statement, contractEnsure)
        {
            Contract.Requires(contractEnsure != null);

            _contractEnsure = contractEnsure;
        }

        public IClrTypeName EnsuresType { get { return _contractEnsure.ResultType; } }

        public override bool AssertsArgumentIsNotNull(string name)
        {
            return true;
        }

        public static ContractEnsuresAssertion TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            
            var invocationExpression = AsInvocationExpression(statement);
            if (invocationExpression == null)
                return null;

            var assertion = ContractEnsureExpression.FromInvocationExpression(invocationExpression);
            if (assertion == null)
                return null;

            return new ContractEnsuresAssertion(statement, assertion);
        }
    }
}