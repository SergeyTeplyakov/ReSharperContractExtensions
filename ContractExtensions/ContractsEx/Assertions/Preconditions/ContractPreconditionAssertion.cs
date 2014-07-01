using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public abstract class ContractPreconditionAssertion : ContractAssertion
    {
        protected ContractPreconditionAssertion(ICSharpStatement statement, ContractAssertionExpressionBase expression)
            : base(AssertionType.Precondition, statement, expression)
        { }

        /// <summary>
        /// Returns true if current precondition is based on Code Contracts library, false otherwise.
        /// </summary>
        public bool IsCodeContractBasedPrecondition
        {
            get
            {
                return PreconditionType == PreconditionType.ContractRequires ||
                       PreconditionType == PreconditionType.GenericContractRequires;
            }
        }

        public abstract PreconditionType PreconditionType { get; }

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

    /// <summary>
    /// Set of extension methods that provides "pattern-matching"-like using of the 
    /// <see cref="ContractPreconditionAssertion"/> objects.
    /// </summary>
    public static class ContractPreconditionAssertionVisitorExtension
    {
        public static void Match(this ContractPreconditionAssertion assertion,
            Action<ContractRequiresPreconditionAssertion> contractRequiresProcessor,
            Action<IfThrowPreconditionAssertion> ifThrowPreconditionProcessor,
            Action<GuardBasedPreconditionAssertion> guardBasePreconditionProcessor)
        {
            Contract.Requires(assertion != null);
            Contract.Requires(contractRequiresProcessor != null);
            Contract.Requires(ifThrowPreconditionProcessor != null);
            Contract.Requires(guardBasePreconditionProcessor != null);

            if (assertion is ContractRequiresPreconditionAssertion)
                contractRequiresProcessor((ContractRequiresPreconditionAssertion) assertion);
            else if (assertion is IfThrowPreconditionAssertion)
                ifThrowPreconditionProcessor((IfThrowPreconditionAssertion)assertion);
            else if (assertion is GuardBasedPreconditionAssertion)
                guardBasePreconditionProcessor((GuardBasedPreconditionAssertion) assertion);

            Contract.Assert(false, "Unknown type of assertion: " + assertion.GetType());
        }

        public static T Match<T>(this ContractPreconditionAssertion assertion,
            Func<ContractRequiresPreconditionAssertion, T> contractRequiresProcessor,
            Func<IfThrowPreconditionAssertion, T> ifThrowPreconditionProcessor,
            Func<GuardBasedPreconditionAssertion, T> guardBasePreconditionProcessor)
        {
            Contract.Requires(assertion != null);
            Contract.Requires(contractRequiresProcessor != null);
            Contract.Requires(ifThrowPreconditionProcessor != null);
            Contract.Requires(guardBasePreconditionProcessor != null);

            if (assertion is ContractRequiresPreconditionAssertion)
                return contractRequiresProcessor((ContractRequiresPreconditionAssertion) assertion);

            if (assertion is IfThrowPreconditionAssertion)
                return ifThrowPreconditionProcessor((IfThrowPreconditionAssertion)assertion);

            if (assertion is GuardBasedPreconditionAssertion)
                return guardBasePreconditionProcessor((GuardBasedPreconditionAssertion) assertion);

            Contract.Assert(false, "Unknown type of assertion: " + assertion.GetType());
            throw new InvalidOperationException("Unknown type of assertion: " + assertion.GetType());
        }


    }
}