using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [ContractClass(typeof(PreconditionProblemAnalyzerContract))]
    public abstract class PreconditionProblemAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        protected override void Run(IInvocationExpression element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var contractAssertion = ContractAssertionExpression.FromInvocationExpression(element) as ContractRequiresExpression;
            if (contractAssertion == null)
                return;

            var highlightings = DoRun(contractAssertion);

            foreach (var highlighting in highlightings)
            {
                consumer.AddHighlighting(
                    highlighting, element.GetDocumentRange(), element.GetContainingFile());
            }
        }

        protected abstract IEnumerable<IHighlighting> DoRun(ContractRequiresExpression contractAssertion);
    }

    [ContractClassFor(typeof (PreconditionProblemAnalyzer))]
    abstract class PreconditionProblemAnalyzerContract : PreconditionProblemAnalyzer
    {
        protected override IEnumerable<IHighlighting> DoRun(ContractRequiresExpression contractAssertion)
        {
            Contract.Requires(contractAssertion != null);
            Contract.Ensures(Contract.Result<IEnumerable<IHighlighting>>() != null);

            throw new System.NotImplementedException();
        }
    }
}