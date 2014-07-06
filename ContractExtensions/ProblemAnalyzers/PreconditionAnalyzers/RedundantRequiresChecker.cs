using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;
using ReSharper.ContractExtensions.Utilities;

[assembly: RegisterConfigurableSeverity(RedundantRequiresCheckerHighlighting.ServerityId,
  null,
  HighlightingGroupIds.CodeSmell,
  RedundantRequiresCheckerHighlighting.ServerityId,
  "Warning for code contract precondition checks",
  Severity.WARNING,
  false)]

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
            var contractAssertion = ContractAssertionExpression.FromInvocationExpression(invocationExpression);
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

    [ConfigurableSeverityHighlighting("RedundantRequiresCheckerHighlighting", CSharpLanguage.Name)]
    public sealed class RedundantRequiresCheckerHighlighting : IHighlighting
    {
        private readonly ICSharpStatement _preconditionStatement;
        private readonly string _argumentName;

        public RedundantRequiresCheckerHighlighting(ICSharpStatement preconditionStatement, string argumentName)
        {
            Contract.Requires(preconditionStatement != null);
            Contract.Requires(!string.IsNullOrEmpty(argumentName));

            _preconditionStatement = preconditionStatement;
            _argumentName = argumentName;
        }

        public const string ServerityId = "RedundantRequiresCheckerHighlighting";
        public const string ToolTipWarning = "Suspicios requires for nullable argument";

        public bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return string.Format("{0} '{1}'", ToolTipWarning, ArgumentName); }
        }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }

        public ICSharpStatement PreconditionStatement
        {
            get
            {
                Contract.Ensures(Contract.Result<ICSharpStatement>() != null);
                return _preconditionStatement;
            }
        }

        public string ArgumentName
        {
            get { return _argumentName; }
        }
    }
}