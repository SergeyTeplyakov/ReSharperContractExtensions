using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

[assembly: RegisterConfigurableSeverity(PreconditionInMethodWithIteratorBlockHighlighing.Id,
  null,
  HighlightingGroupIds.CodeSmell,
  PreconditionInMethodWithIteratorBlockHighlighing.Id,
  "Legacy precondition in methods with iterator block",
  Severity.WARNING,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{

    /// <summary>
    /// Shows errors, produced by Code Contract compiler.
    /// </summary>
    [ConfigurableSeverityHighlighting(Id, CSharpLanguage.Name)]
    public class PreconditionInMethodWithIteratorBlockHighlighing : LegacyContractCustomWarningHighlighting
    {
        public const string Id = "Preconditions in method with iterator block";

        internal PreconditionInMethodWithIteratorBlockHighlighing(CustomWarningValidationResult warning, ValidatedContractBlock contractBlock)
            : base(warning, contractBlock)
        {}
    }
}