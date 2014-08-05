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
        // TODO: is it possible to generialize this?
        private class ExpressionMatcher
        {
            private readonly IExpression _expression;

            public ExpressionMatcher(IExpression expression)
            {
                Contract.Requires(expression != null);

                _expression = expression;
            }

            public T Match<T>(
                Func<IEqualityExpression, T> equalityMatch,
                Func<IUnaryOperatorExpression, T> unaryOpertorMatch,
                Func<IInvocationExpression, T> invocationMatch,
                Func<ICSharpExpression, T> defaultMatch) where T : class
            {
                Contract.Requires(equalityMatch != null);
                Contract.Requires(unaryOpertorMatch != null);
                Contract.Requires(invocationMatch != null);
                Contract.Requires(defaultMatch != null);

                if (_expression is IEqualityExpression)
                    return equalityMatch((IEqualityExpression) _expression);
                if (_expression is IUnaryOperatorExpression)
                    return unaryOpertorMatch((IUnaryOperatorExpression) _expression);
                if (_expression is IInvocationExpression)
                    return invocationMatch((IInvocationExpression) _expression);
                if (_expression is ICSharpExpression)
                    return defaultMatch((ICSharpExpression) _expression);

                return default(T);
            }
        }
        
        /// <summary>
        /// Parses specified <paramref name="expression"/> into sequence of <see cref="PredicateCheck"/>.
        /// </summary>
        public static IEnumerable<PredicateCheck> Create(IExpression expression)
        {
            // TODO: potential enhancement: simplify condition first and convert !(result == null)
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IEnumerable<PredicateCheck>>() != null);

            return
                expression.ProcessRecursively<IExpression>()
                .Select(CreateFrom)
                .Where(pc => pc != null)
                .ToList();
        }

        private static PredicateCheck CreateFrom(IExpression expression)
        {
            Contract.Requires(expression != null);

            var matcher = new ExpressionMatcher(expression);

            return matcher.Match(
                equality => (PredicateCheck) EqualityExpressionPredicateCheck.TryCreate(equality),
                unaryOperator => MethodCallPredicateCheck.TryCreate(unaryOperator),
                invocation => MethodCallPredicateCheck.TryCreate(invocation),
                csharpExpression => null);
        }
    }
}