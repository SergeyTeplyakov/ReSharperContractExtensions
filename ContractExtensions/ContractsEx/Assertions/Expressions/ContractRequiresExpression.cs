using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;

namespace ReSharper.ContractExtensions.ContractsEx
{
    public sealed class ContractRequiresExpression : ContractAssertionExpression
    {
        internal ContractRequiresExpression(AssertionType assertionType, List<PredicateCheck> predicates, IExpression predicateExpression, string message) 
            : base(assertionType, predicates, predicateExpression, message)
        {}

        [CanBeNull]
        public IClrTypeName GenericArgumentType { get; internal set; }
    }
}