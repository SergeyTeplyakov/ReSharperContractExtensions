using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;

namespace ReSharper.ContractExtensions.ContractsEx
{
    internal class RequiresStatement : ContractStatement
    {
        private readonly PreconditionExpression _preconditionExpression;

        private RequiresStatement(ICSharpStatement statement, PreconditionExpression preconditionExpression)
            : base(statement)
        {
            _preconditionExpression = preconditionExpression;
            ArgumentNames = preconditionExpression.PreconditionExpressions.Select(p => p.ArgumentName).ToList();
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_preconditionExpression.IsValid);
            Contract.Invariant(_preconditionExpression.PreconditionType == PreconditionType.Requires);

            Contract.Invariant(ArgumentNames != null);
            Contract.Invariant(ArgumentNames.Count != 0);
        }

        public static RequiresStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            
            // TODO: is it necessary? If so, move to "monadic" implementaion!
            var invokedExpression = AsInvocationExpression(statement);
            if (invokedExpression == null)
                return null;

            var preconditionExpression = PreconditionExpression.Parse(invokedExpression);
            if (!preconditionExpression.IsValid)
                return null;

            return new RequiresStatement(statement, preconditionExpression);
        }

        public IList<string> ArgumentNames { get; private set; }
        public string Message { get { return _preconditionExpression.Message; } }
    }
}