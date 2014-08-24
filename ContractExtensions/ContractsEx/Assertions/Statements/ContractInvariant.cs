using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractInvariant : CodeContractAssertion
    {
        internal ContractInvariant(ICSharpStatement statement, PredicateExpression predicateExpression, Message message) 
            : base(statement, predicateExpression, message)
        {}

        public override ContractAssertionType AssertionType { get { return ContractAssertionType.Invariant; } }
    }
}