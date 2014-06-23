using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    internal static class ContractRequiresPreconditionAssertionEx
    {
        /// <summary>
        /// Returns exception name that could be used for current non-generic requires.
        /// </summary>
        public static IClrTypeName PotentialGenericVersionException(this ContractRequiresPreconditionAssertion assertion)
        {
            Contract.Requires(assertion != null);
            Contract.Requires(!assertion.IsGeneric);
            Contract.Ensures(Contract.Result<IClrTypeName>() != null);

            if (assertion.ContractAssertionExpression.PreconditionExpressions.Any(n => n.ChecksForNotNull()))
            {
                return new ClrTypeName(typeof(ArgumentNullException).FullName);
            }

            return new ClrTypeName(typeof(ArgumentException).FullName);
        }
    }

    public sealed class ContractRequiresPreconditionAssertion : ContractPreconditionAssertion
    {
        private readonly ContractAssertionExpression _contractAssertionExpression;

        private ContractRequiresPreconditionAssertion(ICSharpStatement statement, 
            ContractAssertionExpression contractAssertionExpression)
            : base(statement, contractAssertionExpression.Message)
        {
            Contract.Requires(contractAssertionExpression != null);
            _contractAssertionExpression = contractAssertionExpression;
        }

        public override PreconditionType PreconditionType
        {
            get { return IsGeneric ? PreconditionType.GenericContractRequires : PreconditionType.ContractRequires; }
        }

        public override bool ChecksForNull(string name)
        {
            return _contractAssertionExpression.PreconditionExpressions.Any(n => n.ChecksForNotNull(name));
        }

        internal ContractAssertionExpression ContractAssertionExpression
        {
            get
            {
                Contract.Ensures(Contract.Result<ContractAssertionExpression>() != null);
                return _contractAssertionExpression;
            }
        }

        public bool IsGeneric { get; private set; }

        [CanBeNull]
        new internal static ContractRequiresPreconditionAssertion TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invokedExpression = AsInvocationExpression(statement);
            if (invokedExpression == null)
                return null;

            var assertion = ContractAssertionExpression.FromInvocationExpression(invokedExpression) as ContractRequiresExpression;
            if (assertion != null && assertion.AssertionType == AssertionType.Precondition)
                return new ContractRequiresPreconditionAssertion(statement, assertion)
                {
                    IsGeneric = assertion.GenericArgumentType != null,
                };

            return null;
        }
    }
}