using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Represents precondition like "Guard.NotNull(s)" or similar clauses.
    /// </summary>
    /// <remarks>
    /// The only "guards" that considered as preconditions by Code Contracts library should be 
    /// decorated with ContractArgumentValidatorAttribute.
    /// </remarks>
    public sealed class GuardBasedPreconditionAssertion : ContractPreconditionAssertion
    {
        public GuardBasedPreconditionAssertion(ICSharpStatement statement) 
            : base(statement)
        {}

        public override bool ChecksForNull(string name)
        {
            
            throw new System.NotImplementedException();
        }

        public override bool IsCodeContractBasedPrecondition
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}