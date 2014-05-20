using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.Preconditions.Logic
{
    public enum PreconditionType
    {
        Requires,
        Invariant,
    }

    /// <summary>
    /// Represents "precondition check" such as Contract.Requires or Contract.Invariant.
    /// </summary>
    /// <remarks>
    /// Every valid precondtion contains following:
    /// Contract.Requires(arg != null);
    /// So the parsed version contains:
    /// Precondition type ("Requires
    /// " in this case)
    /// Predicate Left Hand Side ("arg" in this case)
    /// Predicate Equality Type ("!=" in this case) and
    /// Predicate Right Hand Side ("null" in this case)
    /// </remarks>
    internal struct PreconditionExpression
    {
        public static PreconditionExpression Parse(IInvocationExpression invocationExpression)
        {
            // TODO: potential enhancement: simplify condition first and convert !(result == null)
            Contract.Requires(invocationExpression != null);

            PreconditionType? preconditionType = GetPreconditionType(invocationExpression);
            if (preconditionType == null)
                return CreateInvalid();

            Contract.Assert(invocationExpression.Arguments.Count != 0,
                "Precondition expression should have at least one argument!");

            IExpression originalExpression = invocationExpression.Arguments[0].Expression;

            var expression = originalExpression as IEqualityExpression;
            if (expression == null)
                return CreateInvalid();

            var left = expression.LeftOperand as IReferenceExpression;
            
            var right = expression.RightOperand
                .With(x => x as ICSharpLiteralExpression)
                .With(x => x.Literal)
                .Return(x => x.GetText());

            bool isValid = (left != null && right != null && right == "null" 
                && expression.EqualityType == EqualityExpressionType.NE);

            string message = ExtractMessage(invocationExpression);

            return new PreconditionExpression()
            {
                IsValid = isValid,
                
                PredicateLeftSide = left,
                PredicateEqualityType = expression.EqualityType,
                PredicateRightSide = expression.Return(x => x.RightOperand as ILiteralExpression),

                Message = message,
            };
        }

        private static PreconditionType? GetPreconditionType(IInvocationExpression invocationExpression)
        {
            var clrType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();

            if (clrType.Return(x => x.FullName) != typeof (Contract).FullName)
                return null;

            return ParsePreconditionType(method);
        }

        private static PreconditionType? ParsePreconditionType(string method)
        {
            PreconditionType result;
            if (Enum.TryParse(method, true, out result))
                return result;

            return null;
        }

        private static string ExtractMessage(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            Contract.Assert(invocationExpression.Arguments.Count != 0);

            var message = invocationExpression.Arguments.Skip(1).FirstOrDefault()
                .With(x => x.Expression as ICSharpLiteralExpression)
                .With(x => x.Literal.GetText());
            return message;
        }

        public bool IsValid { get; private set; }

        public PreconditionType? PreconditionType { get; private set; }
        public string PredicateArgument { get { return PredicateLeftSide.NameIdentifier.Name; } }
        public string Message { get; private set; }

        private IReferenceExpression PredicateLeftSide { get; set; }
        private EqualityExpressionType PredicateEqualityType { get; set; }
        private ILiteralExpression PredicateRightSide { get; set; }

        private static PreconditionExpression CreateInvalid()
        {
            return new PreconditionExpression() { IsValid = false };
        }
    }
}