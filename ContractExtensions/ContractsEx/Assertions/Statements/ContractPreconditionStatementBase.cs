using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Base class for any precondition statemnts, including Code Contract-based or custom.
    /// </summary>
    public abstract class ContractPreconditionStatementBase : ContractStatementBase
    {
        protected ContractPreconditionStatementBase(ICSharpStatement statement, ContractExpressionBase expression)
            : base(statement, expression)
        { }

        /// <summary>
        /// Returns true if current precondition is based on Code Contracts library, false otherwise.
        /// </summary>
        public abstract bool IsCodeContractBasedPrecondition { get; }

        public abstract PreconditionType PreconditionType { get; }

        /// <summary>
        /// Tries to create <see cref="ContractPreconditionStatementBase"/> from the specified <paramref name="statement"/>.
        /// The result could be <see cref="ContractRequiresStatement"/> or <see cref="IfThrowPreconditionStatement"/>
        /// </summary>
        public static ContractPreconditionStatementBase TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            return ContractStatementFactory.FromCSharpStatement(statement) as ContractPreconditionStatementBase;
        }
    }
}