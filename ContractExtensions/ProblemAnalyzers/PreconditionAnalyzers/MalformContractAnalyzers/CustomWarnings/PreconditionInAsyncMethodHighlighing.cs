using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(PreconditionInAsyncMethodHighlighting.Id,
  null,
  HighlightingGroupIds.CodeSmell,
  PreconditionInAsyncMethodHighlighting.Id,
  "Preconditions in async method",
  Severity.WARNING,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{

    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public class PreconditionInAsyncMethodHighlighting : ContractCustomWarningHighlighting
    {
        public const string Id = "Preconditions in async method";

        internal PreconditionInAsyncMethodHighlighting(CustomWarningValidationResult warning,
            ValidatedContractBlock contractBlock)
            : base(warning, contractBlock)
        {}
    }
}