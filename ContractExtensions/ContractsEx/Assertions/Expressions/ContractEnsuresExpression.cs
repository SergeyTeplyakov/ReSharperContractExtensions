using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    /// <summary>
    /// Represents Contract.Ensures expression.
    /// </summary>
    public sealed class ContractEnsuresExpression : CodeContractExpression
    {
        // TODO: change to sequence of result types!!
        private readonly ContractResultPredicateArgument _contractResultArgument;

        internal ContractEnsuresExpression(IExpression originalPredicateExpression,
            List<PredicateCheck> predicates, Message message) 
            : base(originalPredicateExpression, predicates, message)
        {
            _contractResultArgument =
                predicates.Select(p => p.Argument)
                    .OfType<ContractResultPredicateArgument>()
                    .FirstOrDefault();
        }

        public override AssertionType AssertionType { get { return AssertionType.Ensures; } }

        public IList<IDeclaredType> ContractResultTypes
        {
            get 
            { 
                return 
                    Predicates
                    .Select(p => p.Argument)
                    .OfType<ContractResultPredicateArgument>()
                    .Select(pa => pa.ResultType)
                    .ToList(); 
            }
        }

        [CanBeNull]
        public IDeclaredType DeclaredResultType
        {
            get { return _contractResultArgument.Return(x => x.ResultType); }
        }

        public void SetContractResultType(IType contractResultType)
        {
            Contract.Requires(contractResultType != null);
            Contract.Requires(DeclaredResultType != null, "Nothing to change!");

            foreach (var contractResult in Predicates.Select(p => p.Argument).OfType<ContractResultPredicateArgument>())
            {
                contractResult.SetResultType(contractResultType);
            }
        }

        [CanBeNull]
        public IClrTypeName ResultType
        {
            get { return DeclaredResultType.Return(x => x.GetClrName()); }
        }
    }
}