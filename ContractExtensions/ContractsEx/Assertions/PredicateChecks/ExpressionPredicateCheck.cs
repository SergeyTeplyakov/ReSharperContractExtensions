using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Represents one predicate check in the Contract.Requires statement.
    /// </summary>
    /// <remarks>
    /// For example, n > 0 && n &lt; 42 etc
    /// </remarks>
    // TODO: don't think that we need this!!
    internal sealed class ExpressionPredicateCheck : PredicateCheck
    {
        private readonly ICSharpExpression _expression;

        public ExpressionPredicateCheck(PredicateArgument argument, ICSharpExpression expression)
            : base(argument)
        {
            Contract.Requires(expression != null);
            _expression = expression;
        }

        public ICSharpExpression Expression
        {
            get { return _expression; }
        }

        public override bool ChecksForNotNull()
        {
            return false;
        }

        public override bool ChecksForNull()
        {
            return false;
        }

        [CanBeNull]
        public static ExpressionPredicateCheck TryCreate(ICSharpExpression expression)
        {
            Contract.Requires(expression != null);
            return new ExpressionPredicateCheck(new EmptyPredicateArgument(), expression);
        }
    }
}