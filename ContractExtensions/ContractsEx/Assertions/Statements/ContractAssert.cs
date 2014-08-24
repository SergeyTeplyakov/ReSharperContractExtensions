using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractAssert : CodeContractAssertion
    {
        internal ContractAssert(ICSharpStatement statement, PredicateExpression predicateExpression, Message message)
            : base(statement, predicateExpression, message)
        {}

        public override ContractAssertionType AssertionType { get { return ContractAssertionType.Assert; } }
    }
}