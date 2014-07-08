using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    internal class MethodCallPredicateCheck : PredicateCheck
    {
        protected bool _hasNot;

        protected MethodCallPredicateCheck(PredicateArgument argument, IClrTypeName callSiteType) 
            : base(argument)
        {
            Contract.Requires(callSiteType != null);
            CallSiteType = callSiteType;
        }

        public IClrTypeName CallSiteType { get; private set; }
        public string MethodName { get; protected set; }

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

            if (IsEnum(callSiteType))
                return EnumValidationPredicateCheck.TryCreateEnumPredicateCheck(invocationExpression, hasNot);

            var method = invocationExpression.GetCalledMethod();

            var argument = ExtractArgument(invocationExpression.Arguments.FirstOrDefault().With(x => x.Value));

            argument = argument ?? new EmptyPredicateArgument();

            if (callSiteType == null || method == null)
                return null;

            return new MethodCallPredicateCheck(argument, callSiteType)
            {
                MethodName = method,
                _hasNot = hasNot,
            };
        }

        protected static bool IsEnum(IClrTypeName name)
        {
            return name.With(x => x.FullName) == typeof (System.Enum).FullName;
        }
    }
}