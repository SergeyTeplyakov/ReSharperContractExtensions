using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Represents precondition validator call to the method marked with 
    /// ContractArgumentValidatorAttribute.
    /// </summary>
    public sealed class PreconditionValidator : ContractStatement
    {
        public PreconditionValidator(ICSharpStatement statement, 
            PredicateExpression predicateExpression, Message message) : base(statement, predicateExpression, message)
        {}
    }
}