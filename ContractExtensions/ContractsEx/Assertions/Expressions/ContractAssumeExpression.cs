using System.Collections.Generic;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;

namespace ReSharper.ContractExtensions.ContractsEx
{
    public sealed class ContractAssumeExpression : CodeContractExpression
    {
        internal ContractAssumeExpression(IExpression originalPredicateExpression, 
            List<PredicateCheck> predicates, Message message) 
            : base(originalPredicateExpression, predicates, message)
        {}

        public override AssertionType AssertionType { get { return AssertionType.Assume; } }
    }
}