using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

[assembly: RegisterConfigurableSeverity(RequiresExceptionValidityHighlighting.ServerityId,
  null,
  HighlightingGroupIds.CompilerWarnings,
  RequiresExceptionValidityHighlighting.ServerityId,
  "Warning exception in Contract.Requires<E> doesn't have appropriate constructor",
  Severity.WARNING,
  false)]

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [ConfigurableSeverityHighlighting("RequiresExceptionValidityHighlighting", CSharpLanguage.Name)]
    public sealed class RequiresExceptionValidityHighlighting : IHighlighting
    {
        private readonly IClass _exceptionDeclaration;
        private readonly MemberWithAccess _preconditionContainer;

        internal RequiresExceptionValidityHighlighting(
            IClass exceptionDeclaration,
            MemberWithAccess preconditionContainer)
        {
            Contract.Requires(exceptionDeclaration != null);
            Contract.Requires(preconditionContainer != null);

            _exceptionDeclaration = exceptionDeclaration;
            _preconditionContainer = preconditionContainer;
        }

        public const string ServerityId = "RequiresExceptionValidityHighlighting";
        const string ToolTipWarningFormat = "Exception type '{0}' used in Requires<E> should have ctor(string, string) or ctor(string).\r\nSystem.ArgumentException would be used!";

        public bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get
            {
                return string.Format(ToolTipWarningFormat, _exceptionDeclaration.GetClrName().ShortName); }
        }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }

        internal IClass ExceptionClass { get { return _exceptionDeclaration; } }

        internal MemberWithAccess PreconditionContainer { get { return _preconditionContainer; } }
    }
}