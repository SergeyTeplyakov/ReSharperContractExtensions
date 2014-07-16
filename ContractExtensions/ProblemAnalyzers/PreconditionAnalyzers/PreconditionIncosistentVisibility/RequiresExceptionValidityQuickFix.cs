using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Refactorings.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [QuickFix]
    public sealed class RequiresExceptionValidityQuickFix : QuickFixBase
    {
        private readonly RequiresExceptionValidityHighlighting _highlighting;
        private string _missedConstructor;
        private Func<IConstructor, bool> _missedConstructorPredicate;

        public RequiresExceptionValidityQuickFix(RequiresExceptionValidityHighlighting highlighting)
        {
            Contract.Requires(highlighting != null);

            _highlighting = highlighting;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var exceptionDeclaration = 
                (IClassDeclaration)_highlighting.ExceptionClass.GetDeclarations().FirstOrDefault();
            Contract.Assert(exceptionDeclaration != null);                

            using (var workflow = GeneratorWorkflowFactory.CreateWorkflowWithoutTextControl(
                GeneratorStandardKinds.Constructor,
                exceptionDeclaration,
                exceptionDeclaration))
            {
                Contract.Assert(workflow != null);

                var ctors = workflow.Context.ProvidedElements
                    .OfType<GeneratorDeclaredElement<IConstructor>>()
                    .Where(ge => _missedConstructorPredicate(ge.DeclaredElement))
                    .ToList();

                Contract.Assert(ctors.Count != 0);

                workflow.Context.InputElements.Clear();
                workflow.Context.InputElements.AddRange(ctors);
                workflow.BuildInputOptions();
                workflow.GenerateAndFinish("Generate missing constructor", NullProgressIndicator.Instance);
            }

            return null;
        }

        public override string Text
        {
            get
            {
                return string.Format("Generate {0} for the referenced exception type '{1}'",
                    _missedConstructor, 
                    _highlighting.ExceptionClass.GetClrName().ShortName);
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

            // Fix is available only when exception declared in the same project!!
            if (project == null || exceptionProject == null || project.Guid != exceptionProject.Guid)
                return false;

            var exceptionDeclaration =
                (IClassDeclaration) _highlighting.ExceptionClass.GetDeclarations().FirstOrDefault();

            if (exceptionDeclaration == null)
                return false;

            // TODO: terrible approach, but can't find better!
            using (var workflow = GeneratorWorkflowFactory.CreateWorkflowWithoutTextControl(
                GeneratorStandardKinds.Constructor,
                exceptionDeclaration,
                exceptionDeclaration))
            {
                Contract.Assert(workflow != null);

                var ctors = workflow.Context.ProvidedElements
                    .OfType<GeneratorDeclaredElement<IConstructor>>()
                    .Select(c => c.DeclaredElement)
                    .ToList();

                // TODO: refine me later!!
                if (ctors.Any(ContractUtils.HasTwoStringArgument))
                {
                    _missedConstructorPredicate = ContractUtils.HasTwoStringArgument;
                    _missedConstructor = "ctor(string, string)";
                    return true;
                }

                if (ctors.Any(ContractUtils.HasOneStringArgument))
                {
                    _missedConstructorPredicate = ContractUtils.HasOneStringArgument;
                    _missedConstructor = "ctor(string)";
                    return true;
                }

                return false;
            }
        }
    }
}