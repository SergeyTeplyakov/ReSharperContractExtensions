using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    [QuickFix]
    public sealed class MalformedMethodContractQuickFix : QuickFixBase
    {
        private readonly MalformedContractFix _malformedContractFix;

        private MalformedMethodContractQuickFix(ICodeContractFixableIssue highlighting)
        {
            Contract.Requires(highlighting != null);

            _malformedContractFix = MalformedContractFix.TryCreate(highlighting.CurrentStatement, highlighting.ValidatedContractBlock);
        }

        public MalformedMethodContractQuickFix(CodeContractErrorHighlighting errorHighlighting)
            : this(errorHighlighting as ICodeContractFixableIssue)
        {
        }

        public MalformedMethodContractQuickFix(CodeContractWarningHighlighting warningHighlighting)
            : this(warningHighlighting as ICodeContractFixableIssue)
        {}

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_malformedContractFix != null);
            return _malformedContractFix.ExecuteFix();
        }

        public override string Text
        {
            get
            {
                Contract.Assert(_malformedContractFix != null);
                return _malformedContractFix.FixName;
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _malformedContractFix != null;
        }
    }
}