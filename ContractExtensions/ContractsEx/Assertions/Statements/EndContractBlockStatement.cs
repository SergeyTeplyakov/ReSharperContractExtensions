using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class EndContractBlockStatement : ContractStatementBase
    {
        internal EndContractBlockStatement(ICSharpStatement statement, EndContractBlockExpression expression) 
            : base(statement, expression)
        {}
    }
}