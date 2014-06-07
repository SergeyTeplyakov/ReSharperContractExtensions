using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    internal sealed class MethodCallPredicateCheck : IPredicateCheck
    {
        public IClrTypeName CallSiteType { get; private set; }
        public string MethodName { get; private set; }
        public string ArgumentName { get; private set; }

        public bool ChecksForNull(string name)
        {
            return (name == ArgumentName) &&
                (CallSiteType.FullName == typeof(string).FullName &&
                    (MethodName == "IsNullOrEmpty" || MethodName == "IsNullOrWhiteSpace"));
        }

        [CanBeNull]
        public static MethodCallPredicateCheck TryCreate(IUnaryOperatorExpression expression)
        {
            Contract.Requires(expression != null);

            // Looking for constructs like !string.IsNullOrEmpty or !string.IsNullOrWhitespace

            // TODO: add test case: (!(!(string.IsNullOrEmpty)))

            if (expression.UnaryOperatorType != UnaryOperatorType.EXCL)
            {
                return null;
            }

            var invocationExpression = expression.Operand as IInvocationExpression;
            if (invocationExpression == null)
                return null;

            var callSiteType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();
            var argument = invocationExpression.Arguments.FirstOrDefault()
                .With(x => x.Value as IReferenceExpression)
                .With(x => x.NameIdentifier.Name);

            if (callSiteType == null || method == null || argument == null)
                return null;

            return new MethodCallPredicateCheck
            {
                CallSiteType = callSiteType,
                MethodName = method,
                ArgumentName = argument
            };
        }
    }
}