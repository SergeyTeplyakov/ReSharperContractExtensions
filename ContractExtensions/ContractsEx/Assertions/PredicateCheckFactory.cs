using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    internal static class PredicateCheckFactory
    {
        public static IEnumerable<IPredicateCheck> Create(IExpression expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IEnumerable<IPredicateCheck>>() != null);

            return expression.ProcessRecursively<IEqualityExpression>()
                .Select(ExpressionPredicateCheck.TryCreate)
                .Where(e => e != null)
                .Cast<IPredicateCheck>()
                .Concat(
                    expression.ProcessRecursively<IUnaryOperatorExpression>()
                        .Select(MethodCallPredicateCheck.TryCreate)
                        .Where(e => e != null));
        }
    }
}