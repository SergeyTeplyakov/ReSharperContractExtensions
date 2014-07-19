using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    [ContractClass(typeof(IContractStatementContract))]
    public interface IContractStatement
    {
        ICSharpStatement Statement { get; }
    }

    [ContractClassFor(typeof (IContractStatement))]
    abstract class IContractStatementContract : IContractStatement
    {
        ICSharpStatement IContractStatement.Statement
        {
            get
            {
                Contract.Ensures(Contract.Result<ICSharpStatement>() != null);
                throw new System.NotImplementedException();
            }
        }
    }
}