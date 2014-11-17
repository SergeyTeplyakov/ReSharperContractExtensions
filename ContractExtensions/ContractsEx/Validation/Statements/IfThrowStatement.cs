using System;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Statements
{
    public sealed class IfThrowStatement : ContractStatement
    {
        private IfThrowStatement(ICSharpStatement statement, IfThrowPrecondition contractPrecondition) 
            : base(statement)
        {}

        public static IfThrowStatement TryCreate(ICSharpStatement statement)
        {
            return IfThrowPrecondition.TryCreate(statement).Return(x => new IfThrowStatement(statement, x));
        }

        public override bool IsPrecondition
        {
            get { return true; }
        }
    }

    public static class ContractStatementEx
    {
        public static bool IsIfThrowStatement(this ContractStatement statement)
        {
            return statement is IfThrowStatement;
        }
    }
}