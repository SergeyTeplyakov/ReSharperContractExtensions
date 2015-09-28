using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    [QuickFix]
    public class LegacyContractFix : QuickFixBase
    {
        private readonly LegacyContractCustomWarningHighlighting _legacyContractCustomWarning;

        public LegacyContractFix(LegacyContractCustomWarningHighlighting legacyContractCustomWarning)
        {
            Contract.Requires(legacyContractCustomWarning != null);
            _legacyContractCustomWarning = legacyContractCustomWarning;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            throw new NotImplementedException();
        }

        public override string Text
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            throw new NotImplementedException();
        }
    }
}