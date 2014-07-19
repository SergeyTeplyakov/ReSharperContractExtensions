using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ContractEnsuresStatement : ContractStatementBase
    {
        private readonly ContractEnsuresExpression _contractEnsures;

        internal ContractEnsuresStatement(ICSharpStatement statement, ContractEnsuresExpression contractEnsures) 
            : base(statement, contractEnsures)
        {
            _contractEnsures = contractEnsures;
        }

        [CanBeNull]
        public IClrTypeName EnsuresType { get { return _contractEnsures.ResultType; } }

        public override bool AssertsArgumentIsNotNull(string name)
        {
            return true;
        }

        public static ContractEnsuresStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            return ContractStatementFactory.FromCSharpStatement(statement) as ContractEnsuresStatement;
        }
    }
}