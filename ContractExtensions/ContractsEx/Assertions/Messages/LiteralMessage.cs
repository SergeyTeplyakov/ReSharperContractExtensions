using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class LiteralMessage : Message
    {
        private readonly string _literal;

        public LiteralMessage(IExpression originalExpression, string literal)
            : base(originalExpression)
        {
            Contract.Requires(!string.IsNullOrEmpty(literal));
            _literal = literal;
        }

        public string Literal
        {
            get { return _literal; }
        }
    }
}