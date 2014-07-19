using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Statements
{
    /// <summary>
    /// Base class for every statement that uses <see cref="Contract"/> class method.
    /// </summary>
    public abstract class ContractStatement
    {
        [CanBeNull]
        public static ContractStatement Create(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);


            return null;
        }
    }
}