using System;
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
        private static readonly Func<IExpression, IEnumerable<PredicateCheck>> _equalityExpressionFactory;
        private static readonly Func<IExpression, IEnumerable<PredicateCheck>> _unaryOperatorExpressionFactory;
        private static readonly Func<IExpression, IEnumerable<PredicateCheck>> _invocationExpressionFactory;
        private static readonly Func<IExpression, IEnumerable<PredicateCheck>> _csharpExpressionFactory;

        static PredicateCheckFactory()
        {
            _equalityExpressionFactory = 
                ex => ex.ProcessRecursively<IEqualityExpression>()
                        .Select(EqualityExpressionPredicateCheck.TryCreate)
                        .Where(n => n != null);

            _unaryOperatorExpressionFactory =
                ex => ex.ProcessRecursively<IUnaryOperatorExpression>()
                        .Select(MethodCallPredicateCheck.TryCreate)
                        .Where(n => n != null);

            _invocationExpressionFactory = 
                ex => ex.ProcessRecursively<IInvocationExpression>()
                        .Select(MethodCallPredicateCheck.TryCreate)
                        .Where(n => n != null);

            _csharpExpressionFactory =
                ex => ex.ProcessRecursively<ICSharpExpression>()
                    .Select(ExpressionPredicateCheck.TryCreate)
                    .Where(n => n != null);
        }

        public static IEnumerable<PredicateCheck> Create(IExpression expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IEnumerable<PredicateCheck>>() != null);

            var factory = GetFactoryMethodFor(expression);
            return factory(expression);
        }

        private static Func<IExpression, IEnumerable<PredicateCheck>> GetFactoryMethodFor(IExpression expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<Func<IExpression, IEnumerable<PredicateCheck>>>() != null);

            if (expression is IEqualityExpression)
                return _equalityExpressionFactory;

            if (expression is IUnaryOperatorExpression)
                return _unaryOperatorExpressionFactory;

            if (expression is IInvocationExpression)
                return _invocationExpressionFactory;

            return _csharpExpressionFactory;
        }
    }
}