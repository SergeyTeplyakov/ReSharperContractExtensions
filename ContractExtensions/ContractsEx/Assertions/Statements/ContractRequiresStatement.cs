using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractRequiresStatement : ContractPreconditionStatementBase
    {
        private readonly ContractRequiresExpression _contractExpression;

        internal ContractRequiresStatement(ICSharpStatement statement, 
            ContractRequiresExpression contractExpression)
            : base(statement, contractExpression)
        {
            _contractExpression = contractExpression;

            IsGeneric = contractExpression.GenericArgumentType != null;
        }

        public override bool IsCodeContractBasedPrecondition
        {
            get { return true; }
        }

        public override PreconditionType PreconditionType
        {
            get { return IsGeneric ? PreconditionType.GenericContractRequires : PreconditionType.ContractRequires; }
        }

        internal ContractRequiresExpression ContractRequiresExpression
        {
            get
            {
                Contract.Ensures(Contract.Result<CodeContractExpression>() != null);
                return _contractExpression;
            }
        }

        public bool IsGeneric { get; private set; }

        [CanBeNull]
        new internal static ContractRequiresStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invokedExpression = AsInvocationExpression(statement);
            if (invokedExpression == null)
                return null;

            var assertion = CodeContractExpression.FromInvocationExpression(invokedExpression) as ContractRequiresExpression;
            if (assertion == null)
                return null;

            return new ContractRequiresStatement(statement, assertion);
        }
    }

    internal static class ContractRequiresPreconditionAssertionEx
    {
        /// <summary>
        /// Returns exception name that could be used for current non-generic requires.
        /// </summary>
        public static IClrTypeName PotentialGenericVersionException(this ContractRequiresStatement assertion)
        {
            Contract.Requires(assertion != null);
            Contract.Requires(!assertion.IsGeneric);
            Contract.Ensures(Contract.Result<IClrTypeName>() != null);

            if (assertion.ContractRequiresExpression.Predicates.Any(n => n.ChecksForNotNull()))
            {
                return new ClrTypeName(typeof(ArgumentNullException).FullName);
            }

            return new ClrTypeName(typeof(ArgumentException).FullName);
        }
    }
}