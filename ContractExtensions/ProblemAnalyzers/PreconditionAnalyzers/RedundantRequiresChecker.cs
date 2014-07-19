using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    /// <summary>
    /// Warns Code Contract user that Contract.Requires statement checks for null for the obviously nullable
    /// arguments, like Foo(string s = null) or Foo([CanBeNull]string s);
    /// </summary>
    [ElementProblemAnalyzer(new[] { typeof(IInvocationExpression) }, 
        HighlightingTypes = new[] { typeof(RedundantRequiresCheckerHighlighting) })]
    public sealed class RedundantRequiresChecker : ElementProblemAnalyzer<IInvocationExpression>
    {
        protected override void Run(IInvocationExpression element, 
            ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            string argumentName;
            if (IsAvailable(element, out argumentName))
            { 
                consumer.AddHighlighting(
                    new RedundantRequiresCheckerHighlighting(element.GetContainingStatement(), argumentName), 
                    element.GetDocumentRange(), element.GetContainingFile());
            }
        }

        private bool IsAvailable(IInvocationExpression invocationExpression, out string argumentName)
        {
            argumentName = null;

            var contractAssertion = CodeContractExpression.FromInvocationExpression(invocationExpression) as CodeContractExpression;
            if (contractAssertion == null)
                return false;

            Func<ReferenceArgument, IParameter> paramSelector =
                ra =>
                {
                    var r = ra.With(x => x.ReferenceExpression)
                        .With(x => x.Reference)
                        .With(x => x.Resolve())
                        .With(x => x.DeclaredElement as IParameter);
                    return r;
                };

            Func<IParameter, bool> isNullableOrDefault =
                pd => pd.IsDefaultedToNull() || pd.HasClrAttribute(typeof(CanBeNullAttribute));

            var result = 
                contractAssertion.Predicates
                .Where(p => p.ChecksForNotNull())
                .Select(p => p.Argument)
                .OfType<ReferenceArgument>()
                .FirstOrDefault(p => paramSelector(p).With(x => (bool?) isNullableOrDefault(x)) == true);

            argumentName = result.With(x => x.BaseArgumentName);
            return argumentName != null;
        }
    }
}