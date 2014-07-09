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
        private static readonly Dictionary<Type, Func<IExpression, IEnumerable<PredicateCheck>>> _factories = 
            new Dictionary<Type, Func<IExpression, IEnumerable<PredicateCheck>>>();

        static PredicateCheckFactory()
        {
            _factories[typeof(IEqualityExpression)] = 
                ex => ex.ProcessRecursively<IEqualityExpression>()
                        .Select(EqualityExpressionPredicateCheck.TryCreate)
                        .Where(n => n != null);

            _factories[typeof(IUnaryOperatorExpression)] = 
                ex => ex.ProcessRecursively<IUnaryOperatorExpression>()
                        .Select(MethodCallPredicateCheck.TryCreate)
                        .Where(n => n != null);

            _factories[typeof(IInvocationExpression)] = 
                ex => ex.ProcessRecursively<IInvocationExpression>()
                        .Select(MethodCallPredicateCheck.TryCreate)
                        .Where(n => n != null);
        }

        public static IEnumerable<PredicateCheck> Create(IExpression expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IEnumerable<PredicateCheck>>() != null);

            var factory = GetFactoryMethodFor(expression);
            return factory(expression);
        }


        private static Func<IExpression, IEnumerable<PredicateCheck>> CSharpExpressionPredicateCheck
        {
            get
            {
                return ex => ex.ProcessRecursively<ICSharpExpression>()
                    .Select(ExpressionPredicateCheck.TryCreate)
                    .Where(n => n != null);
            }
        }

        private static Func<IExpression, IEnumerable<PredicateCheck>> GetFactoryMethodFor(IExpression expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<Func<IExpression, IEnumerable<PredicateCheck>>>() != null);


            Func<IExpression, IEnumerable<PredicateCheck>> result;
            if (_factories.TryGetValue(expression.GetType(), out result))
                return result;

            return CSharpExpressionPredicateCheck;
        }
    }
}