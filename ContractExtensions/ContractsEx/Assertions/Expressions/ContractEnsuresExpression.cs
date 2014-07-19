using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    /// <summary>
    /// Represents Contract.Ensures expression.
    /// </summary>
    public sealed class ContractEnsuresExpression : CodeContractExpression
    {
        internal ContractEnsuresExpression(IExpression originalPredicateExpression,
            List<PredicateCheck> predicates, Message message) 
            : base(originalPredicateExpression, predicates, message)
        {
            ResultType =
                predicates.Select(p => p.Argument)
                    .OfType<ContractResultPredicateArgument>()
                    .FirstOrDefault()
                    .Return(x => x.ResultTypeName);
        }

        public override AssertionType AssertionType { get { return AssertionType.Ensures; } }

        //[CanBeNull]
        //public static ContractEnsuresExpression FromInvocationExpression(IInvocationExpression invocationExpression)
        //{
        //    Contract.Requires(invocationExpression != null);

        //    AssertionType? assertionType = GetContractAssertionType(invocationExpression);
        //    if (assertionType == null || assertionType != AssertionType.Ensures)
        //        return null;

        //    Contract.Assert(invocationExpression.Arguments.Count != 0,
        //        "Requires expression should have at least one argument!");

        //    IExpression originalExpression = invocationExpression.Arguments[0].Expression;
        //    var predicates = PredicateCheckFactory.Create(originalExpression).ToList();

        //    if (predicates.Count == 0)
        //        return null;

        //    return new ContractEnsuresExpression(predicates, ExtractMessage(invocationExpression));
        //}

        [CanBeNull]
        public IClrTypeName ResultType { get; private set; }

        //[CanBeNull]
        //private static IDeclaredType ExtractContractResultType(IInvocationExpression contractResultExpression)
        //{
        //    if (contractResultExpression == null)
        //        return null;

        //    var callSiteType = contractResultExpression.GetCallSiteType();
        //    var method = contractResultExpression.GetCalledMethod();

        //    if (callSiteType.With(x => x.FullName) != typeof(Contract).FullName ||
        //        method != "Result")
        //        return null;

        //    return contractResultExpression
        //        .With(x => x.InvokedExpression)
        //        .With(x => x as IReferenceExpression)
        //        .With(x => x.TypeArguments.FirstOrDefault())
        //        .Return(x => x as IDeclaredType);
        //}
    }
}