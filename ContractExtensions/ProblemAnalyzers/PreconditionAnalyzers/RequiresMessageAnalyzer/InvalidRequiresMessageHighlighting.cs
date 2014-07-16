using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

[assembly: RegisterConfigurableSeverity(InvalidRequiresMessageHighlighting.ServerityId,
  null,
  HighlightingGroupIds.CompilerWarnings, //CC1065
  RedundantRequiresCheckerHighlighting.ServerityId,
  "Warn if Contract.Requires uses non-constant string as a message",
  Severity.ERROR,
  false)]

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    /// <summary>
    /// This is high
    /// </summary>
    [ConfigurableSeverityHighlighting("InvalidRequiresMessageHighlighting", CSharpLanguage.Name)]
    public sealed class InvalidRequiresMessageHighlighting : IHighlighting
    {
        private readonly ICSharpStatement _preconditionStatement;
        private readonly string _argumentName;

        public InvalidRequiresMessageHighlighting(ICSharpStatement preconditionStatement, string argumentName)
        {
            Contract.Requires(preconditionStatement != null);
            Contract.Requires(!string.IsNullOrEmpty(argumentName));

            _preconditionStatement = preconditionStatement;
            _argumentName = argumentName;
        }

        public const string ServerityId = "InvalidRequiresMessageHighlighting";
        // error CC1065: User message to contract call can only be string literal, 
        // or a static field, or static property that is at least internally visible.
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