using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [QuickFix]
    public sealed class RedundantRequiresQuickFix : QuickFixBase
    {
        private readonly RedundantRequiresCheckerHighlighting _highlighting;

        public RedundantRequiresQuickFix(RedundantRequiresCheckerHighlighting highlighting)
        {
            Contract.Requires(highlighting != null);

            _highlighting = highlighting;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            _highlighting.PreconditionStatement.RemoveOrReplaceByEmptyStatement();
            
            return null;
        }

        public override string Text
        {
            get { return string.Format("Remove redundant precondition for argument '{0}'", 
                _highlighting.ArgumentName); }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return true;
        }
    }
}