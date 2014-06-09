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
    /// For example, s != null, str.Length == 42 etc
    /// </remarks>
    internal sealed class ExpressionPredicateCheck : IPredicateCheck
    {
        public string ArgumentName { get; private set; }
        public EqualityExpressionType EqualityType { get; private set; }
        public ICSharpLiteralExpression RightHandSide { get; private set; }

        public bool ChecksForNotNull(string name)
        {
            return ArgumentName == name && 
                   (EqualityType == EqualityExpressionType.NE && 
                    RightHandSide.Literal.GetText() == "null");
        }

        public bool ChecksForNull(string name)
        {
            return ArgumentName == name &&
                   (EqualityType == EqualityExpressionType.EQEQ &&
                    RightHandSide.Literal.GetText() == "null");
        }

        [CanBeNull]
        public static ExpressionPredicateCheck TryCreate(IEqualityExpression expression)
        {
            Contract.Requires(expression != null);

            var left = expression.LeftOperand as IReferenceExpression;

            var right = expression.RightOperand as ICSharpLiteralExpression;

            if (left == null || right == null)
                return null;

            // The problem is, that for "person.Name != null" and
            // for "person != null" I should get "person"
            var qualifierReference = left.QualifierExpression
                .With(x => x as IReferenceExpression);

            string predicateArgument = (qualifierReference ?? left).NameIdentifier.Name;

            return new ExpressionPredicateCheck
            {
                ArgumentName = predicateArgument,
                EqualityType = expression.EqualityType,
                RightHandSide = right,
            };
        }
    }
}