using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Impl
{
    internal sealed class InvocationVisitor : TreeNodeVisitor
    {
        public IInvocationExpression InvocationExpression { get; private set; }

        public override void VisitInvocationExpression(IInvocationExpression invocationExpression)
        {
            InvocationExpression = invocationExpression;
            base.VisitInvocationExpression(invocationExpression);
        }
    }
}