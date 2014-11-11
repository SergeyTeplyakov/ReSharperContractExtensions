using System.Linq;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{

    /// <summary>
    /// Warns if method contract is malformed:
    /// - Contract statements are not the first statements in the method
    /// - Ensures statement is after precondition check
    /// </summary>
    [ElementProblemAnalyzer(new[] { typeof(ICSharpFunctionDeclaration) },
    HighlightingTypes = new[] { typeof(ContractCustomWarningHighlighting), typeof(CodeContractWarningHighlighting) })]
    public sealed class MalformedMethodContractChecker : ElementProblemAnalyzer<ICSharpFunctionDeclaration>
    {
        protected override void Run(ICSharpFunctionDeclaration element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            // Right now there is two different rule sets for Code Contract statements and for any preconditions.
            var validateContractBlock = CodeContractBlockValidator.ValidateCodeContractBlock(element.GetCodeContractBlockStatements());

            var preconditions = CodeContractBlockValidator.ValidateAllPreconditions(element.GetContractBlockStatements());

            foreach (var vr in validateContractBlock.ValidationResults.Union(preconditions.ValidationResults))
            {
                var highlighting = vr.Match(
                    error => (IHighlighting)new CodeContractErrorHighlighting(error, validateContractBlock),
                    warning => new CodeContractWarningHighlighting(warning, validateContractBlock),
                    customWarning => ContractCustomWarningHighlighting.Create(customWarning, validateContractBlock),
                    _ => null);

                if (highlighting != null)
                {
                    consumer.AddHighlighting(highlighting, vr.Statement.GetDocumentRange(), element.GetContainingFile());
                }
            }
        }
    }
}