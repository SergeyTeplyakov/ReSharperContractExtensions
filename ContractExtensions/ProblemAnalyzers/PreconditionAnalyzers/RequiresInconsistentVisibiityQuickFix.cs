using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.Utilities;

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
            // TODO: R# documentation saying that GetDeclarations is VERY expensive!
            // Can I do the same but in some other way???
            var declaration = _highlighting.EnclosingMember.DeclaredElement.GetDeclarations().FirstOrDefault();

            Contract.Assert(declaration != null);

            ModifiersUtil.SetAccessRights(
                declaration,
                _highlighting.PreconditionContainer.AccessRights);

            return null;
        }

        public override string Text
        {
            get
            {
                var enclosingMember = _highlighting.EnclosingMember;

                return string.Format("Change visibility of the enclosing {0} '{1}' to '{2}'", 
                    enclosingMember.MemberTypeName, enclosingMember.MemberName, 
                    _highlighting.PreconditionContainer.AccessRights.ToCSharpString());
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return BelongToTheSameType(_highlighting.PreconditionContainer, _highlighting.EnclosingMember);
        }

        private bool BelongToTheSameType(MemberWithAccess preconditionContainer, MemberWithAccess enclosingMember)
        {
            var preconditionContainingType =
                preconditionContainer.DeclaredElement
                    .With(x => x as IClrDeclaredElement)
                    .Return(x => x.GetContainingType());

            var enclosingMemberContiningType =
                enclosingMember.DeclaredElement
                    .With(x => x as IClrDeclaredElement)
                    .Return(x => x.GetContainingType());

            if (preconditionContainingType == null || enclosingMemberContiningType == null)
                return false;

            return preconditionContainingType.GetClrName().FullName ==
                   enclosingMemberContiningType.GetClrName().FullName;
        }
    }
}