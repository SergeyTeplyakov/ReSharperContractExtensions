using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractAssumeStatement : ContractStatementBase
    {
        internal ContractAssumeStatement(ICSharpStatement statement, ContractAssumeExpression expression) 
            : base(statement, expression)
        {}
    }
}