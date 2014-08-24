using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public static class ContractStatementFactory
    {
        [CanBeNull]
        public static IPrecondition TryCreatePrecondition(ICSharpStatement statement)
        {
            var ifThrowPrecondition = IfThrowPrecondition.TryCreate(statement);
            if (ifThrowPrecondition != null)
                return ifThrowPrecondition;

            var requires = CodeContractAssertion.TryCreate(statement) as IPrecondition;
            if (requires != null)
                return requires;

            return null;
        }

        [CanBeNull]
        public static CodeContractAssertion TryCreateAssertion(ICSharpStatement statement)
        {
            return CodeContractAssertion.TryCreate(statement);
        }

        [CanBeNull]
        public static CodeContractAssertion TryCreateAssertion(IInvocationExpression invocationExpression)
        {
            return CodeContractAssertion.FromInvocationExpression(invocationExpression);
        }
    }
}