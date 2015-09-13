using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [ContractClass(typeof(PreconditionProblemAnalyzerContract))]
    public abstract class PreconditionProblemAnalyzer : ElementProblemAnalyzer<IInvocationExpression>
    {
        protected override sealed void Run(IInvocationExpression element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var contractAssertion = CodeContractAssertion.FromInvocationExpression(element) as ContractRequires;
            if (contractAssertion == null)
                return;

            var preconditionContainer = element
                .With(x => x.GetContainingStatement())
                .With(x => x.GetContainingTypeMemberDeclaration())
                .With(x => x.DeclaredElement)
                .With(MemberWithAccess.FromDeclaredElement);
            if (preconditionContainer == null)
                return;

            var highlightings = DoRun(element, contractAssertion, preconditionContainer);

            foreach (var highlighting in highlightings)
            {
                consumer.AddHighlighting(
                    highlighting, element.GetDocumentRange(), element.GetContainingFile());
            }
        }

        protected abstract IEnumerable<IHighlighting> DoRun(IInvocationExpression invocationExpression,
            ContractRequires contractAssertion, 
            MemberWithAccess preconditionContainer);
    }

    [ContractClassFor(typeof (PreconditionProblemAnalyzer))]
    abstract class PreconditionProblemAnalyzerContract : PreconditionProblemAnalyzer
    {
        protected override IEnumerable<IHighlighting> DoRun(IInvocationExpression invocationExpression, 
            ContractRequires contractAssertion, MemberWithAccess preconditionContainer)
        {
            Contract.Requires(invocationExpression != null);
            Contract.Requires(contractAssertion != null);
            Contract.Requires(preconditionContainer != null);
            Contract.Ensures(Contract.Result<IEnumerable<IHighlighting>>() != null);
            throw new System.NotImplementedException();
        }
    }
}