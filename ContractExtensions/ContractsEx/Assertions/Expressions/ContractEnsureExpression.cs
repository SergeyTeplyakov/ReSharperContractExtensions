using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;

namespace ReSharper.ContractExtensions.ContractsEx
{
    internal sealed class ContractEnsureExpression : ContractAssertionExpressionBase
    {
        private readonly List<PredicateCheck> _predicates;

        public ContractEnsureExpression(List<PredicateCheck> predicates, string message) 
            : base(AssertionType.Postcondition, predicates, message)
        {
            Contract.Requires(predicates != null);
            Contract.Requires(predicates.Count != 0);
            _predicates = predicates;

            ResultType = 
                _predicates.Select(p => p.Argument)
                .OfType<ContractResultPredicateArgument>()
                .First()
                .ResultTypeName;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(ResultType != null);
        }

        [CanBeNull]
        public static ContractEnsureExpression FromInvocationExpression(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            AssertionType? assertionType = GetAssertionType(invocationExpression);
            if (assertionType == null || assertionType != AssertionType.Postcondition)
                return null;

            Contract.Assert(invocationExpression.Arguments.Count != 0,
                "Precondition expression should have at least one argument!");

            IExpression originalExpression = invocationExpression.Arguments[0].Expression;
            var predicates = PredicateCheckFactory.Create(originalExpression).ToList();

            if (predicates.Count == 0)
                return null;

            //var expression = originalExpression as IEqualityExpression;
            //if (expression == null)
            //    return null;

            //// looking for type from expression like: Contract.Result<Type>()
            //var ensureType = ExtractContractResultType(
            //    expression.LeftOperand.With(x => x as IInvocationExpression));

            //var right = expression.RightOperand
            //    .With(x => x as ICSharpLiteralExpression)
            //    .With(x => x.Literal)
            //    .Return(x => x.GetText());

            //if (ensureType == null || right == null || right != "null" &&
            //    expression.EqualityType != EqualityExpressionType.NE)
            //{
            //    return null;
            //}

            return new ContractEnsureExpression(predicates, ExtractMessage(invocationExpression));
        }

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