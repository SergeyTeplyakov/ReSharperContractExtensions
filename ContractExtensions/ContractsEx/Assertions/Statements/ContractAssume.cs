using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractAssume : CodeContractAssertion
    {
        internal ContractAssume(ICSharpStatement statement, PredicateExpression predicateExpression, Message message) 
            : base(statement, predicateExpression, message)
        {}

        public override ContractAssertionType AssertionType { get { return ContractAssertionType.Assume; } }
    }
}