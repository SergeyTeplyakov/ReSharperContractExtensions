using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public abstract class ContractPreconditionAssertion : ContractAssertion
    {
        protected ContractPreconditionAssertion(ICSharpStatement statement)
            : base(AssertionType.Precondition, statement)
        { }

        /// <summary>
        /// Returns true if current precondition is based on Code Contracts library, false otherwise.
        /// </summary>
        public abstract bool IsCodeContractBasedPrecondition { get; }

        /// <summary>
        /// Tries to create <see cref="ContractPreconditionAssertion"/> from the specified <paramref name="statement"/>.
        /// The result could be <see cref="ContractRequiresPreconditionAssertion"/> or <see cref="IfThrowPreconditionAssertion"/>
        /// </summary>
        public static ContractPreconditionAssertion TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var requiresAssertion = ContractRequiresPreconditionAssertion.TryCreate(statement);
            if (requiresAssertion != null)
                return requiresAssertion;

            return IfThrowPreconditionAssertion.TryCreate(statement);
        }
    }
}