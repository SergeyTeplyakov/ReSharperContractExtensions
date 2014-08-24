using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Represents message in the contract expression.
    /// </summary>
    /// <remarks>
    /// This hierarchy is an OO implementation of the descriminated union with a list of following cases:
    /// <see cref="NoMessage"/>, <see cref="LiteralMessage"/>, <see cref="ReferenceMessage"/> and 
    /// <see cref="InvocationMessage"/>.
    /// </remarks>
    public abstract class Message
    {
        private readonly IExpression _originalExpression;

        protected Message(IExpression originalExpression)
        {
            _originalExpression = originalExpression;
        }

        public static Message Create(IExpression expression)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<Message>() != null);

            var literal = expression as ICSharpLiteralExpression;
            if (literal != null)
                return new LiteralMessage(expression, literal.Literal.GetText());

            var reference = expression as IReferenceExpression;
            if (reference != null)
                return new ReferenceMessage(expression, reference);

            var invocationExpression = expression as IInvocationExpression;
            if (invocationExpression != null)
                return new InvocationMessage(expression, invocationExpression);

            return NoMessage.Instance;
        }

        public IExpression OriginalExpression { get { return _originalExpression; } }
    }
}