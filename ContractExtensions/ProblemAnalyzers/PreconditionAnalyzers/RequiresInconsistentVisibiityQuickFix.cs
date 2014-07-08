using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [QuickFix]
    public sealed class RequiresInconsistentVisibiityQuickFix : QuickFixBase
    {
        private readonly RequiresInconsistentVisibiityHighlighting _highlighting;

        public RequiresInconsistentVisibiityQuickFix(RequiresInconsistentVisibiityHighlighting highlighting)
        {
            Contract.Requires(highlighting != null);

            _highlighting = highlighting;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            //_highlighting.PreconditionStatement.RemoveOrReplaceByEmptyStatement();
            
            return null;
        }

        public override string Text
        {
            get { return string.Format("Change availability '{0}'", 
                "foo"); }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            // We can check, if the other is current file, then we can fix this!
            return true;
        }
    }
}