using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx
{
    internal sealed class InvariantStatement : ContractStatement
    {
        private InvariantStatement(ICSharpStatement statement, 
            PreconditionExpression preconditionExpression)
            : base(statement)
        {
            Contract.Requires(preconditionExpression.IsValid);

            ArgumentNames = preconditionExpression.PreconditionExpressions.Select(x => x.ArgumentName).ToList();
            Message = preconditionExpression.Message;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(ArgumentNames != null);
            Contract.Invariant(ArgumentNames.Count != 0);
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

        public IList<string> ArgumentNames { get; private set; }
        public string Message { get; private set; }
    }
}