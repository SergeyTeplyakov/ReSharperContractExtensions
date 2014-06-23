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
        public GuardBasedPreconditionAssertion(ICSharpStatement statement, string message) 
            : base(statement, message)
        {}

        public override bool ChecksForNull(string name)
        {
            
            throw new System.NotImplementedException();
        }

        public override PreconditionType PreconditionType
        {
            get { return PreconditionType.GuardClause; }
        }
    }
}