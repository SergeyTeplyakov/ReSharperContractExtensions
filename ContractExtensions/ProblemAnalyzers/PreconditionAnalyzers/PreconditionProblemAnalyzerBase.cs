using System.Collections.Generic;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    public abstract class PreconditionProblemAnalyzerBase<T> : ElementProblemAnalyzer<T> where T : ITreeNode
    {
        protected sealed override void Run(T element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var highlightings = DoRun(element);

            foreach (var highlighting in highlightings)
            {
                consumer.AddHighlighting(
                    highlighting, element.GetDocumentRange(), element.GetContainingFile());
            }
        }

        protected abstract IEnumerable<IHighlighting> DoRun(T element);
    }
}