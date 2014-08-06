namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// Represents issue with Code Contract Statement.
    /// </summary>
    internal interface ICodeContractFixableIssue
    {
        ValidationResult CurrentStatement { get; }
        ValidatedContractBlock ValidatedContractBlock { get; }
    }
}