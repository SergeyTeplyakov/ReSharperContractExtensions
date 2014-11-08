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
            var contractBlockStatements = element.GetContractBlockStatements();

            if (contractBlockStatements.Count == 0)
                return;

            var validateContractBlock = ContractBlockValidator.ValidateContractBlock(contractBlockStatements);

            foreach (var vr in validateContractBlock.ValidationResults)
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