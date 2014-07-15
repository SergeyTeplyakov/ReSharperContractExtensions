using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [QuickFix]
    public sealed class RequiresExceptionInconsistentVisibilityQuickFix : QuickFixBase
    {
        private readonly RequiresExceptionInconsistentVisibiityHighlighting _highlighting;

        public RequiresExceptionInconsistentVisibilityQuickFix(RequiresExceptionInconsistentVisibiityHighlighting highlighting)
        {
            Contract.Requires(highlighting != null);

            _highlighting = highlighting;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var declaration =
                _highlighting.ExceptionClass.GetDeclarations().FirstOrDefault();

            Contract.Assert(declaration != null);

            ModifiersUtil.SetAccessRights(
                declaration,
                _highlighting.PreconditionContainer.TypeAccessRights);

            return null;
        }

        public override string Text
        {
            get
            {
                return string.Format("Change visibility of the referenced exception type '{0}' to '{1}'",
                    _highlighting.ExceptionClass.GetClrName().ShortName,
                    _highlighting.PreconditionContainer.TypeAccessRights.ToCSharpString());
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            // Quick fix should be available only for exceptions, declared in the same project.
            // I.e. fix would be unavailable for internal exeptions visible because of InternalVisibleTo
            var project =
                _highlighting.PreconditionContainer.DeclaredElement
                    .With(x => x.GetContainingType())
                    .With(x => x.GetSingleOrDefaultSourceFile())
                    .With(x => x.GetProject());

            var exceptionProject =
                _highlighting.ExceptionClass
                    .With(x => x.GetDeclarations().FirstOrDefault())
                    .With(x => x.GetProject());

            if (project == null || exceptionProject == null)
                return false;

            return project.Guid == exceptionProject.Guid;
        }
    }
}