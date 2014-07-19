using System.Linq;
using ReSharper.ContractExtensions.ContractsEx.Assertions;

namespace ReSharper.ContractExtensions.ContractsEx
{
    // TODO: design of this stuff still seems ugly!!
    public sealed class EndContractBlockExpression : ContractExpressionBase, ICodeContractExpression
    {
        public EndContractBlockExpression() 
            : base(Enumerable.Empty<PredicateCheck>().ToList(), NoMessage.Instance)
        {
        }

        public AssertionType AssertionType
        {
            get { return AssertionType.EndContractBlock; }
        }
    }
}