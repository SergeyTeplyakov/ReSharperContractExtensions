using System.Diagnostics.Contracts;

namespace ReSharper.ContractExtensions.ContractsEx.Statements
{
    /// <summary>
    /// Base class for every contract statements with expressions, including Requires, Ensures.
    /// </summary>
    public abstract class ExpressionBasedContractStatement : ContractStatement
    {
        private readonly ContractExpressionBase _expression;

        protected ExpressionBasedContractStatement(ContractExpressionBase expression)
        {
            Contract.Requires(expression != null);

            _expression = expression;
        }

        public ContractExpressionBase Expression
        {
            get { return _expression; }
        }
    }
}