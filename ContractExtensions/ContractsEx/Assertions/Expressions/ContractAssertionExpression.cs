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
    /// Represents one Assertion from Code Contract library, like Contract.Requires, Contract.Invariant etc.
    /// </summary>
    /// <remarks>
    /// Every valid precondtion contains following:
    /// Contract.Method(predicates, message).
    /// 
    /// Note that this class is not suitable for Contract.Ensures because it has slightly 
    /// different internal structure.
    /// </remarks>
    public class ContractAssertionExpression : ContractAssertionExpressionBase
    {
        private readonly IExpression _predicateExpression;

        protected ContractAssertionExpression(AssertionType assertionType, List<PredicateCheck> predicates, 
            IExpression predicateExpression, string message)
            : base(assertionType, predicates, message)
        {
            Contract.Requires(predicateExpression != null);

            _predicateExpression = predicateExpression;
        }

        [CanBeNull]
        public static ContractAssertionExpression FromInvocationExpression(
            IInvocationExpression invocationExpression)
        {
            // TODO: potential enhancement: simplify condition first and convert !(result == null)
            Contract.Requires(invocationExpression != null);
            
            AssertionType? assertionType = GetAssertionType(invocationExpression);
            if (assertionType == null)
                return null;

            Contract.Assert(invocationExpression.Arguments.Count != 0,
                "Precondition expression should have at least one argument!");

            IExpression originalExpression = invocationExpression.Arguments[0].Expression;

            var predicates = PredicateCheckFactory.Create(originalExpression).ToList();

            //if (predicates.Count == 0)
            //    return null;

            if (assertionType == AssertionType.Precondition)
            {
                var genericRequiresType = GetGenericRequiresType(invocationExpression);
                return new ContractRequiresExpression(assertionType.Value, predicates, originalExpression,
                    ExtractMessage(invocationExpression))
                {
                    GenericArgumentDeclaredType = genericRequiresType,
                };
            }

            return new ContractAssertionExpression(assertionType.Value, predicates, originalExpression, 
                ExtractMessage(invocationExpression));
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

        public IExpression PredicateExpression
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpression>() != null);
                return _predicateExpression;
            }
        }
    }
}