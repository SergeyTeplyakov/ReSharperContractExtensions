using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class InvocationMessage : Message
    {
        private readonly IInvocationExpression _invocationExpression;

        public InvocationMessage(IExpression originalExpression, IInvocationExpression invocationExpression)
            : base(originalExpression)
        {
            Contract.Requires(invocationExpression != null);

            _invocationExpression = invocationExpression;
        }

        public IInvocationExpression InvocationExpression
        {
            get { return _invocationExpression; }
        }
    }
}