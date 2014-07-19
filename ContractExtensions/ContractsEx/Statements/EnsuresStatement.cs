namespace ReSharper.ContractExtensions.ContractsEx.Statements
{
    public sealed class EnsuresStatement : ExpressionBasedContractStatement
    {
        private readonly ContractEnsuresExpression _ensuresesExpression;

        public EnsuresStatement(ContractEnsuresExpression ensuresesExpression) 
            : base(ensuresesExpression)
        {
            _ensuresesExpression = ensuresesExpression;
        }

        public ContractEnsuresExpression EnsuresesExpression
        {
            get { return _ensuresesExpression; }
        }
    }
}