using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    public sealed class ContractRequiresExpression : ContractAssertionExpression
    {
        internal ContractRequiresExpression(AssertionType assertionType, List<PredicateCheck> predicates, 
            IExpression predicateExpression, string message) 
            : base(assertionType, predicates, predicateExpression, message)
        {}

        [CanBeNull]
        public IDeclaredType GenericArgumentDeclaredType { get; internal set; }

        [CanBeNull]
        public IClrTypeName GenericArgumentType
        {
            get { return GenericArgumentDeclaredType.With(x => x.GetClrName()); }
        }

    }
}