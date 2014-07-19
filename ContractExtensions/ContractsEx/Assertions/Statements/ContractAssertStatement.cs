using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractAssertStatement : ContractStatementBase
    {
        internal ContractAssertStatement(ICSharpStatement statement, ContractAssertExpression expression) 
            : base(statement, expression)
        {}
    }
}