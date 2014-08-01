using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Statements
{
    /// <summary>
    /// Base class for any contract statements including Code Contract statements
    /// like Contract.Esnures, contract abbreviation contract, if-throw contracts or
    /// guard-based contracts.
    /// </summary>
    public abstract class ContractStatement
    {
        protected readonly ICSharpStatement _statement;

        protected ContractStatement(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            _statement = statement;
        }
    }
}