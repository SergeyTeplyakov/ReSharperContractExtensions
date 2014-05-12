using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.Preconditions.Logic
{
    internal sealed class InvariantStatement : ContractStatement
    {
        private InvariantStatement(ICSharpStatement statement, 
            PreconditionExpression preconditionExpression)
            : base(statement)
        {
            Contract.Requires(preconditionExpression.IsValid);

            ArgumentName = preconditionExpression.PredicateArgument;
            Message = preconditionExpression.Message;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(ArgumentName != null);
        }

        public static InvariantStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invocationExpression = AsInvocationExpression(statement);

            var preconditionExpression = PreconditionExpression.Parse(invocationExpression);

            if (!preconditionExpression.IsValid)
                return null;

            return new InvariantStatement(statement, preconditionExpression);
        }

        public string ArgumentName { get; private set; }
        public string Message { get; private set; }
    }
}