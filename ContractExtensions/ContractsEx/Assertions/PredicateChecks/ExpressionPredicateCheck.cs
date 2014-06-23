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
    internal sealed class ExpressionPredicateCheck : PredicateCheck
    {
        private readonly IBinaryExpression _binaryExpression;

        public ExpressionPredicateCheck(string argumentName, IBinaryExpression binaryExpression)
            : base(argumentName)
        {
            Contract.Requires(binaryExpression != null);
            _binaryExpression = binaryExpression;
        }

        public IBinaryExpression BinaryExpression
        {
            get { return _binaryExpression; }
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
        public static ExpressionPredicateCheck TryCreate(IBinaryExpression expression)
        {
            Contract.Requires(expression != null);

            return new ExpressionPredicateCheck("fake", expression);
        }
    }
}