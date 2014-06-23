using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    internal sealed class MethodCallPredicateCheck : PredicateCheck
    {
        private bool _hasNot;

        private MethodCallPredicateCheck(string argumentName) : base(argumentName)
        {
        }

        public IClrTypeName CallSiteType { get; private set; }
        public string MethodName { get; private set; }

        public override bool ChecksForNotNull()
        {
            return _hasNot &&
                (CallSiteType.FullName == typeof(string).FullName &&
                    (MethodName == "IsNullOrEmpty" || MethodName == "IsNullOrWhiteSpace"));
        }

        public override bool ChecksForNull()
        {
            return CallSiteType.FullName == typeof (string).FullName &&
                   (MethodName == "IsNullOrEmpty" || MethodName == "IsNullOrWhiteSpace");
        }

        [CanBeNull]
        public static MethodCallPredicateCheck TryCreate(IInvocationExpression expression)
        {
            return TryCreateImpl(expression, false);
        }

        [CanBeNull]
        public static MethodCallPredicateCheck TryCreate(IUnaryOperatorExpression expression)
        {
            Contract.Requires(expression != null);

            // Looking for constructs like !string.IsNullOrEmpty or !string.IsNullOrWhitespace

            // TODO: add test case: (!(!(string.IsNullOrEmpty)))

            var invocationExpression = expression.Operand as IInvocationExpression;
            if (invocationExpression == null)
                return null;

            return TryCreateImpl(invocationExpression, expression.UnaryOperatorType == UnaryOperatorType.EXCL);
        }

        private static MethodCallPredicateCheck TryCreateImpl(IInvocationExpression invocationExpression,
            bool hasNot)
        {
            var callSiteType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();
            var argument = invocationExpression.Arguments.FirstOrDefault()
                .With(x => x.Value as IReferenceExpression)
                .With(x => x.NameIdentifier.Name);

            if (callSiteType == null || method == null || argument == null)
                return null;

            return new MethodCallPredicateCheck(argument)
            {
                CallSiteType = callSiteType,
                MethodName = method,
                _hasNot = hasNot,
            };
        }
    }
}