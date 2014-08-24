using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Represents Contract.Requires or Contract.Requires&lt;E&gt;.
    /// </summary>
    public sealed class ContractRequires : CodeContractAssertion, IPrecondition
    {
        internal ContractRequires(ICSharpStatement statement, IInvocationExpression invocationExpression,
            PredicateExpression predicateExpression, Message message)
            : base(statement, predicateExpression, message)
        {
            GenericArgumentDeclaredType = GetGenericRequiresType(invocationExpression);
        }

        public override ContractAssertionType AssertionType { get {return ContractAssertionType.Requires;} }

        [CanBeNull]
        public IDeclaredType GenericArgumentDeclaredType { get; private set; }

        [CanBeNull]
        public IClrTypeName GenericArgumentType
        {
            get { return GenericArgumentDeclaredType.With(x => x.GetClrName()); }
        }

        public bool IsGenericRequires
        {
            get { return GenericArgumentType != null; }
        }

        /// <summary>
        /// Returns exception name that could be used for current non-generic requires.
        /// </summary>
        public IClrTypeName PotentialGenericVersionException()
        {
            Contract.Requires(!IsGenericRequires);
            Contract.Ensures(Contract.Result<IClrTypeName>() != null);

            if (Predicates.Any(n => n.ChecksForNotNull()))
            {
                return new ClrTypeName(typeof(ArgumentNullException).FullName);
            }

            return new ClrTypeName(typeof(ArgumentException).FullName);
        }


        [System.Diagnostics.Contracts.Pure]
        [CanBeNull]
        private static IDeclaredType GetGenericRequiresType(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            return invocationExpression.Reference
                .With(x => x.Invocation)
                .With(x => x.TypeArguments.FirstOrDefault())
                .Return(x => x as IDeclaredType);
        }

        public PreconditionType PreconditionType
        {
            get { return IsGenericRequires ? PreconditionType.GenericContractRequires : PreconditionType.ContractRequires; }
        }

        public bool ChecksForNotNull(string name)
        {
            return Predicates.Any(p => p.ChecksForNotNull(name));
        }
    }
}