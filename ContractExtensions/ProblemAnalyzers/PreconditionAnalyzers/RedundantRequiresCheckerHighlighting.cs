using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

[assembly: RegisterConfigurableSeverity(RedundantRequiresCheckerHighlighting.ServerityId,
  null,
  HighlightingGroupIds.CodeSmell,
  RedundantRequiresCheckerHighlighting.ServerityId,
  "Warning for code contract precondition checks",
  Severity.WARNING,
  false)]

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
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