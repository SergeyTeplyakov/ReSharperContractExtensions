using System.Diagnostics.Contracts;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

[assembly: RegisterConfigurableSeverity(RequiresInconsistentVisibiityHighlighting.ServerityId,
  null,
  HighlightingGroupIds.CompilerWarnings, // this is actually a Code Contract compiler error CC1038
  RequiresInconsistentVisibiityHighlighting.ServerityId,
  "Warning for inconsistent visibility members in the precondition check",
  Severity.ERROR,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [ConfigurableSeverityHighlighting("RequiresInconsistentVisibilityHighlighting", CSharpLanguage.Name)]
    public sealed class RequiresInconsistentVisibiityHighlighting : IHighlighting
    {
        private readonly MemberWithAccess _preconditionContainer;
        private readonly MemberWithAccess _lessVisibleReferencedMember;
        private readonly DocumentRange _range;

        internal RequiresInconsistentVisibiityHighlighting(ICSharpStatement preconditionStatement, 
            MemberWithAccess preconditionContainer, MemberWithAccess lessVisibleReferencedMember)
        {
            Contract.Requires(preconditionStatement != null);
            Contract.Requires(preconditionContainer != null);
            Contract.Requires(lessVisibleReferencedMember != null);

            _range = preconditionStatement.GetHighlightingRange();
            _preconditionContainer = preconditionContainer;
            _lessVisibleReferencedMember = lessVisibleReferencedMember;
        }

        public const string ServerityId = "RequiresInconsistentVisibilityHighlighting";
        // Sample: Member 'ConsoleApplication6.Demo.Analyzers.Demo.Check' has less visibility than the enclosing method 'ConsoleApplication6.Demo.Analyzers.Demo.Foo(System.String)'.
        const string ToolTipWarningFormat = "Member '{0}' has less visibility than the enclosing {1} '{2}'";

        public bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// Calculates range of a highlighting.
        /// </summary>
        public DocumentRange CalculateRange()
        {
            return _range;
        }

        internal MemberWithAccess PreconditionContainer { get { return _preconditionContainer; } }
        internal MemberWithAccess LessVisibleReferencedMember { get { return _lessVisibleReferencedMember; } }

        public string ToolTip
        {
            get
            {
                // For members from different type TypeName.Member notation should be used!
                string referencedName = _lessVisibleReferencedMember.MemberName;
                string preconditionContainerName = _preconditionContainer.MemberName;

                if (!_lessVisibleReferencedMember.BelongToTheSameType(_preconditionContainer))
                {
                    referencedName = _lessVisibleReferencedMember.TypeAndMemberName;
                    preconditionContainerName = _preconditionContainer.TypeAndMemberName;
                }

                return string.Format(ToolTipWarningFormat, referencedName, 
                    _preconditionContainer.MemberTypeString, preconditionContainerName); }
        }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }
    }
}