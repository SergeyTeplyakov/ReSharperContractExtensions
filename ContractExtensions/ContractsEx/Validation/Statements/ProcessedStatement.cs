using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Statements
{
    /// <summary>
    /// Represents processed statement from the contract block.
    /// </summary>
    public sealed class ProcessedStatement
    {
        public ICSharpStatement CSharpStatement { get; private set; }

        [CanBeNull]
        public ContractStatement ContractStatement { get; private set; }

        [CanBeNull]
        public CodeContractStatement CodeContractStatement { get { return ContractStatement as CodeContractStatement; } }

        public static ProcessedStatement Create(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            Contract.Ensures(Contract.Result<ProcessedStatement>() != null);

            return new ProcessedStatement
            {
                CSharpStatement = statement,
                ContractStatement = ContractStatementFactory.TryCreate(statement),
            };
        }

    }
}