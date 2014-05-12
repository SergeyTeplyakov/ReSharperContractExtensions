using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.Preconditions.Logic
{
    struct RequiresExpression
    {
        public static RequiresExpression Parse(IExpression originalExpression)
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

            return new RequiresExpression()
            {
                IsValid = true,

                Left = expression.LeftOperand as ILiteralExpression,
                Right = expression.RightOperand,
                EqualityType = expression.EqualityType,
            };
        }

        public bool IsValid { get; private set; }

        public ILiteralExpression Left { get; private set; }
        public EqualityExpressionType EqualityType { get; private set; }
        public ICSharpExpression Right { get; private set; }

        private static RequiresExpression CreateInvalid()
        {
            return new RequiresExpression() { IsValid = false };
        }
    }
}