using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Represents one predicate check in the Contract.Requires statement.
    /// </summary>
    /// <remarks>
    /// For example, s != null, str.Length == 42 etc
    /// </remarks>
    internal sealed class EqualityExpressionPredicateCheck : PredicateCheck
    {
        private EqualityExpressionPredicateCheck(PredicateArgument argument) 
            : base(argument)
        {}

        public EqualityExpressionType EqualityType { get; private set; }
        public ICSharpLiteralExpression RightHandSide { get; private set; }

        public override bool ChecksForNotNull()
        {
            // returns true for "argument != null"
            return EqualityType == EqualityExpressionType.NE && 
                   RightHandSide.Literal.GetText() == "null";
        }

        public override bool ChecksForNull()
        {
            // returns true for "argument == null"
            return EqualityType == EqualityExpressionType.EQEQ &&
                RightHandSide.Literal.GetText() == "null";
        }

        [CanBeNull]
        public static EqualityExpressionPredicateCheck TryCreate(IEqualityExpression expression)
        {
            Contract.Requires(expression != null);

            var left = ExtractArgument(expression.LeftOperand);

            // TODO: right hand side could be any arbitrary expression, not only literals
            var right = expression.RightOperand as ICSharpLiteralExpression;

            if (left == null || right == null)
                return null;

            // The problem is, that for "person.Name != null" and
            // for "person != null" I should get "person"
            //var qualifierReference = left.QualifierExpression
            //    .With(x => x as IReferenceExpression);

            //string predicateArgument = (qualifierReference ?? left).NameIdentifier.Name;

            return new EqualityExpressionPredicateCheck(left)
            {
                EqualityType = expression.EqualityType,
                RightHandSide = right,
            };
        }
    }
}