using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Predicate expression is part of any assertion.
    /// </summary>
    /// <remarks>
    /// For instance: in Contract.Requires(x != null), predicate expression would be "x != null".
    /// In if (x == null) throw new ArgumentException, predicate expression would be "x == null".
    /// </remarks>
    public sealed class PredicateExpression
    {
        private readonly IExpression _originalPredicateExpression;
        private readonly List<PredicateCheck> _predicateChecks;

        private PredicateExpression(IExpression originalPredicateExpression, List<PredicateCheck> predicateChecks)
        {
            Contract.Requires(originalPredicateExpression != null);
            Contract.Requires(predicateChecks != null);

            _originalPredicateExpression = originalPredicateExpression;
            _predicateChecks = predicateChecks;
        }

        public ReadOnlyCollection<PredicateCheck> Predicates
        {
            get { return _predicateChecks.AsReadOnly(); }
        }

        public bool IsChecksArgumentForNull(Func<PredicateArgument, bool> comparer)
        {
            return _predicateChecks.Any(pc => pc.ChecksForNull() && comparer(pc.Argument));
        }

        public bool IsChecksArgumentForNotNull(Func<PredicateArgument, bool> comparer)
        {
            return _predicateChecks.Any(pc => pc.ChecksForNotNull() && comparer(pc.Argument));
        }

        public IExpression OriginalPredicateExpression { get { return _originalPredicateExpression; } }

        public static PredicateExpression Create(IExpression expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<PredicateExpression>() != null);

            var predicates = PredicateCheckFactory.Create(expression).ToList();
            return new PredicateExpression(expression, predicates);
        }
    }
}