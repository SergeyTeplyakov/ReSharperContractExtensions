using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    /// <summary>
    /// Represents Contract.Ensures as a parsed expression.
    /// </summary>
    struct EnsureExpression
    {
        public static EnsureExpression Parse(IExpression originalExpression)
        {
            // TODO: potential enhancement: simplify condition first and convert !(result == null)
            Contract.Requires(originalExpression != null);

            var expression = originalExpression as IEqualityExpression;
            if (expression == null)
                return CreateInvalid();

            var left = expression.LeftOperand
                .With(x => x as IInvocationExpression)
                .With(x => x.InvokedExpression)
                .With(x => x as IReferenceExpression)
                .With(x => x.TypeArguments.FirstOrDefault())
                .With(x => x as IDeclaredType);
            
            var right = expression.RightOperand
                .With(x => x as ICSharpLiteralExpression)
                .With(x => x.Literal)
                .With(x => x.GetText());

            if (left == null || right == null || right != "null" &&
                expression.EqualityType != EqualityExpressionType.NE)
            {
                return CreateInvalid();
            }
            
            return new EnsureExpression
            {
                IsValid = true,

                Left = expression.LeftOperand as IInvocationExpression,
                Right = expression.RightOperand,
                EqualityType = expression.EqualityType,

                ResultType = left,
            };
        }

        public bool IsValid { get; private set; }

        public IDeclaredType ResultType { get; private set; }

        public IInvocationExpression Left { get; private set; }
        public EqualityExpressionType EqualityType { get; private set; }
        public ICSharpExpression Right { get; private set; }

        private static EnsureExpression CreateInvalid()
        {
            return new EnsureExpression() {IsValid = false};
        }
    }
}