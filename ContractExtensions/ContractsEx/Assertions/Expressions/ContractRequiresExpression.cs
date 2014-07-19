using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    /// <summary>
    /// Represents Contract.Requires or Contract.Requires&lt;E&gt;.
    /// </summary>
    public sealed class ContractRequiresExpression : CodeContractExpression
    {
        internal ContractRequiresExpression(IInvocationExpression invocationExpression,
            IExpression originalPredicateExpression,
            List<PredicateCheck> predicates, Message message)
            : base(originalPredicateExpression, predicates, message)
        {
            GenericArgumentDeclaredType = GetGenericRequiresType(invocationExpression);
        }

        public override AssertionType AssertionType { get {return AssertionType.Requires;} }

        [CanBeNull]
        public IDeclaredType GenericArgumentDeclaredType { get; private set; }

        [CanBeNull]
        public IClrTypeName GenericArgumentType
        {
            get { return GenericArgumentDeclaredType.With(x => x.GetClrName()); }
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


    }
}