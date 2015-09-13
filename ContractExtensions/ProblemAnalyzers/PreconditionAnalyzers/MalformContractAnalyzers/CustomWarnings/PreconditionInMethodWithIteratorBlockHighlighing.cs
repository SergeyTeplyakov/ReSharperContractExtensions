using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
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

        internal PreconditionInMethodWithIteratorBlockHighlighing(ICSharpFunctionDeclaration element, CustomWarningValidationResult warning, ValidatedContractBlock contractBlock)
            : base(element, warning, contractBlock)
        {}
    }
}