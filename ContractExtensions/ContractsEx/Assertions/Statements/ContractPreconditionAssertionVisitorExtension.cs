using System;
using System.Diagnostics.Contracts;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Set of extension methods that provides "pattern-matching"-like using of the 
    /// <see cref="ContractPreconditionStatementBase"/> objects.
    /// </summary>
    public static class ContractPreconditionAssertionVisitorExtension
    {
        public static void Match(this ContractPreconditionStatementBase assertion,
            Action<ContractRequiresStatement> contractRequiresProcessor,
            Action<IfThrowPreconditionStatement> ifThrowPreconditionProcessor,
            Action<GuardBasedPreconditionStatement> guardBasePreconditionProcessor)
        {
            Contract.Requires(assertion != null);
            Contract.Requires(contractRequiresProcessor != null);
            Contract.Requires(ifThrowPreconditionProcessor != null);
            Contract.Requires(guardBasePreconditionProcessor != null);

            if (assertion is ContractRequiresStatement)
                contractRequiresProcessor((ContractRequiresStatement) assertion);
            else if (assertion is IfThrowPreconditionStatement)
                ifThrowPreconditionProcessor((IfThrowPreconditionStatement)assertion);
            else if (assertion is GuardBasedPreconditionStatement)
                guardBasePreconditionProcessor((GuardBasedPreconditionStatement) assertion);

            Contract.Assert(false, "Unknown type of assertion: " + assertion.GetType());
        }

        public static T Match<T>(this ContractPreconditionStatementBase assertion,
            Func<ContractRequiresStatement, T> contractRequiresProcessor,
            Func<IfThrowPreconditionStatement, T> ifThrowPreconditionProcessor,
            Func<GuardBasedPreconditionStatement, T> guardBasePreconditionProcessor)
        {
            Contract.Requires(assertion != null);
            Contract.Requires(contractRequiresProcessor != null);
            Contract.Requires(ifThrowPreconditionProcessor != null);
            Contract.Requires(guardBasePreconditionProcessor != null);

            if (assertion is ContractRequiresStatement)
                return contractRequiresProcessor((ContractRequiresStatement) assertion);

            if (assertion is IfThrowPreconditionStatement)
                return ifThrowPreconditionProcessor((IfThrowPreconditionStatement)assertion);

            if (assertion is GuardBasedPreconditionStatement)
                return guardBasePreconditionProcessor((GuardBasedPreconditionStatement) assertion);

            Contract.Assert(false, "Unknown type of assertion: " + assertion.GetType());
            throw new InvalidOperationException("Unknown type of assertion: " + assertion.GetType());
        }


    }
}