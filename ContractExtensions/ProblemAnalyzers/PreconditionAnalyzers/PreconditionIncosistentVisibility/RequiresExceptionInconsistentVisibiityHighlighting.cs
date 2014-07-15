using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

[assembly: RegisterConfigurableSeverity(RequiresExceptionInconsistentVisibiityHighlighting.ServerityId,
  null,
  HighlightingGroupIds.CompilerWarnings, // CC1037
  RequiresExceptionInconsistentVisibiityHighlighting.ServerityId,
  "Warning for less accessible exceptions in Contract.Requires<E>",
  Severity.ERROR,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [ConfigurableSeverityHighlighting("RequiresExceptionInconsistentVisibiityHighlighting", CSharpLanguage.Name)]
    public sealed class RequiresExceptionInconsistentVisibiityHighlighting : IHighlighting
    {
        private readonly IClass _exceptionDeclaration;
        private readonly MemberWithAccess _preconditionContainer;

        internal RequiresExceptionInconsistentVisibiityHighlighting(
            IClass exceptionDeclaration, MemberWithAccess preconditionContainer)
        {
            Contract.Requires(exceptionDeclaration != null);
            Contract.Requires(preconditionContainer != null);

            _exceptionDeclaration = exceptionDeclaration;
            _preconditionContainer = preconditionContainer;
        }

        public const string ServerityId = "RequiresExceptionInconsistentVisibiityHighlighting";
        // Sample: Exception type CodeContractInvestigations.CustomException in Requires<E> has less visibility than enclosing method CodeContractInvestigations.InconsistentPreconditionVisibility.InconsistentAccessibilityIssues
        const string ToolTipWarningFormat = "Exception type '{0}' in Requires<E> has less visibility than the enclosing {1} '{2}'";

        public bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get
            {
                return string.Format(ToolTipWarningFormat, _exceptionDeclaration.GetClrName().ShortName, 
                    _preconditionContainer.MemberTypeString, _preconditionContainer.TypeAndMemberName); }
        }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }

        internal IClass ExceptionClass { get { return _exceptionDeclaration; } }
        internal MemberWithAccess PreconditionContainer { get { return _preconditionContainer; } }
    }
}