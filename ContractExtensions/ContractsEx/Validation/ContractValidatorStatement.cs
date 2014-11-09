using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Statements
{
    /// <summary>
    /// Statement with a method call, marked with ContractArgumentValidatorAttribute.
    /// </summary>
    public sealed class ContractValidatorStatement : ContractStatement
    {
        private ContractValidatorStatement(ICSharpStatement statement) 
            : base(statement)
        {}

        internal static ContractValidatorStatement TryCreate(ICSharpStatement statement, IMethod method)
        {
            Contract.Requires(statement != null);

            return new ContractValidatorStatement(statement);
        }

        public override bool IsPrecondition
        {
            get { return true; }
        }
    }
}