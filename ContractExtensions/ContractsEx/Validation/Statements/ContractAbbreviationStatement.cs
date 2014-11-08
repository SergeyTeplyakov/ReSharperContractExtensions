using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Statements
{
    /// <summary>
    /// Represents contract statement with a call to the method marked with ContractAbbreviatorAttribute.
    /// </summary>
    public sealed class ContractAbbreviationStatement : ContractStatement
    {
        private ContractAbbreviationStatement(ICSharpStatement statement) 
            : base(statement)
        {}

        internal static ContractAbbreviationStatement TryCreate(ICSharpStatement statement, IMethod method)
        {
            Contract.Requires(method != null);

            return new ContractAbbreviationStatement(statement);
        }

        public override bool IsPrecondition
        {
            get { return true; }
        }
    }
}