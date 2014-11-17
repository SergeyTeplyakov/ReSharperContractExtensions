using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public enum PreconditionType
    {
        GenericContractRequires,
        ContractRequires,
        IfThrowStatement
    }

    /// <summary>
    /// Marker interface for any type of precondition: including if-throw, guards and Contract.Requires.
    /// </summary>
    public interface IPrecondition
    {
        ICSharpStatement CSharpStatement { get; }
        PreconditionType PreconditionType { get; }

        /// <summary>
        /// Returns true if current Assertion checks for null something with specified <paramref name="name"/>.
        /// </summary>
        bool ChecksForNotNull(string name);
    }
}