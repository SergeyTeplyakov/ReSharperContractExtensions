using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    internal sealed class EnumValidationPredicateCheck : MethodCallPredicateCheck
    {
        private EnumValidationPredicateCheck(PredicateArgument argument, IClrTypeName callSiteType) 
            : base(argument, callSiteType)
        {
            Contract.Assert(IsEnum(callSiteType));
        }

        public IClrTypeName CheckedEnumTypeName { get; private set; }

        public override bool ChecksForNotNull()
        {
            return MethodName == "IsDefined";
        }

        public override bool ChecksForNull()
        {
            return _hasNot && MethodName == "IsDefined";
        }

        [System.Diagnostics.Contracts.Pure]
        [CanBeNull]
        public static EnumValidationPredicateCheck TryCreateEnumPredicateCheck(IInvocationExpression invocationExpression, bool hasNot)
        {
            Contract.Requires(invocationExpression != null);
            
            // Expected expression: 
            // Enum.IsDefined(typeof(System.Reflection.BindingFlags), flags)

            var callSiteType = invocationExpression.GetCallSiteType();

            Contract.Assert(IsEnum(callSiteType));
            var method = invocationExpression.GetCalledMethod();
            var arguments = invocationExpression.Arguments.ToList();

            if (arguments.Count != 2)
                return null;

            // First argument is typeof(EnumType)
            var enumType =
                arguments[0].Value
                    .With(x => x as ITypeofExpression)
                    .With(x => x.ArgumentType)
                    .With(x => x.GetClrTypeName());

            // Second argument is a enum value itself
            var argument = ExtractArgument(arguments[1].Value);

            if (argument == null)
                return null;

            return new EnumValidationPredicateCheck(argument, callSiteType)
            {
                CheckedEnumTypeName = enumType,
                MethodName = method,
                _hasNot = hasNot,
            };
        }
    }
}