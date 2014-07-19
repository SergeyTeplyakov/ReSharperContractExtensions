namespace ReSharper.ContractExtensions.ContractsEx.Statements
{
    public sealed class RequiresStatement : ExpressionBasedContractStatement
    {
        private readonly ContractRequiresExpression _expression;

        public RequiresStatement(ContractRequiresExpression expression)
            : base(expression)
        {
            _expression = expression;
        }

        public ContractRequiresExpression RequiresExpression
        {
            get { return _expression; }
        }
    }
}