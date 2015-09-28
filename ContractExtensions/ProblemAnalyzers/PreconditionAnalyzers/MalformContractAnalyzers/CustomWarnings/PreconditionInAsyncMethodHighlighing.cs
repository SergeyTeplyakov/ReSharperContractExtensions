using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(PreconditionInAsyncMethodHighlighting.Id,
  null,
  HighlightingGroupIds.CodeSmell,
  PreconditionInAsyncMethodHighlighting.Id,
  "Legacy preconditions in async method",
  Severity.WARNING,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{

    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public class PreconditionInAsyncMethodHighlighting : LegacyContractCustomWarningHighlighting
    {
        public const string Id = "Preconditions in async method";

        internal PreconditionInAsyncMethodHighlighting(ICSharpFunctionDeclaration element, CustomWarningValidationResult warning, ValidatedContractBlock contractBlock)
            : base(element, warning, contractBlock)
        {}
    }
}