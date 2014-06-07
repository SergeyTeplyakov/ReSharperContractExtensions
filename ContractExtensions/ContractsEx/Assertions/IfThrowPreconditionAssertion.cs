using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class IfThrowPreconditionAssertion : ContractPreconditionAssertion
    {
        private IfThrowPreconditionAssertion(ICSharpStatement statement) : base(statement)
        {}

        public override bool IsCodeContractBasedPrecondition
        {
            get { return false; }
        }

        public override bool ChecksForNull(string name)
        {
            return false;
        }

        new internal static IfThrowPreconditionAssertion TryCreate(ICSharpStatement statement)
        {
            return null;
        }
    }
}