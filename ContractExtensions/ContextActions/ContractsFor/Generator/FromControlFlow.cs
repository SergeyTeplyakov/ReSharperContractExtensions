using System;
using System.Collections.Generic;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Features.Altering.Generate;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.UI.Application;
using JetBrains.UI.Application.Progress;
using JetBrains.UI.Tooltips;
using JetBrains.Util;
using JetBrains.Util.Logging;

namespace JetBrains.ReSharper.Intentions2.CSharp.QuickFixes
{
    public class ImplementMemberFix : IQuickFix, IBulbAction
    {
        [CanBeNull]
        private readonly IClassLikeDeclaration myTypeDeclaration;

        public ImplementMemberFix(AbstractInheritedMemberIsNotImplementedError error)
        {
            myTypeDeclaration = error.Declaration as IClassLikeDeclaration;
        }

        public ImplementMemberFix(InterfaceMembersNotImplementedError error)
        {
            myTypeDeclaration = error.Declaration as IClassLikeDeclaration;
        }

        public string Text
        {
            get { return "Implement members"; }
        }

        public IEnumerable<IntentionAction> CreateBulbItems()
        {
            return this.ToQuickFixAction();
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return myTypeDeclaration != null && myTypeDeclaration.IsValid();
        }

        public void Execute(ISolution solution, ITextControl textControl)
        {
            using (var workflow = GeneratorWorkflowFactory.CreateWorkflow(
              GeneratorStandardKinds.Implementations, solution, textControl, CSharpLanguage.Instance))
            {
                var isTestShell = Shell.Instance.IsTestShell;
                if (workflow == null)
                {
                    if (isTestShell || Shell.Instance.IsInInternalMode) Logger.Fail("Workflow is null");

                    Shell.Instance.Components.Tooltips().Show(
                      "Nothing to implement", lifetime => new TextControlPopupWindowContext(lifetime, textControl,
                        Shell.Instance.GetComponent<IShellLocks>(), Shell.Instance.GetComponent<IActionManager>()));

                    return;
                }

                workflow.Context.Anchor = myTypeDeclaration;
                workflow.Context.InputElements.Clear();
                workflow.Context.InputElements.AddRange(workflow.Context.ProvidedElements);

                workflow.Context.SetGlobalOptionValue(
                  CSharpBuilderOptions.ImplementationKind,
                  CSharpBuilderOptions.ImplementationKindPublicMember);

                if (isTestShell || workflow.Context.ProvidedElements.Count == 1)
                {
                    workflow.BuildInputOptions();
                    if (isTestShell)
                    {
                        workflow.GenerateAndFinish(Text, NullProgressIndicator.Instance);
                    }
                    else
                    {
                        Shell
                          .Instance.GetComponent<UITaskExecutor>()
                          .SingleThreaded.ExecuteTask("Implement members", TaskCancelable.Yes,
                            pi => workflow.GenerateAndFinish(Text, pi));
                    }
                }
                else
                {
                    //using (var workflowForm = new GeneratorWorkflowForm(
                    //  Shell.Instance.GetComponent<UIApplication>(), workflow, "Implement members",
                    //  "Select members, for which to generate missing implementations and overrides."))
                    //{
                    //    try
                    //    { workflowForm.ShowDialog(); }
                    //    catch (Exception ex)
                    //    { Logger.LogException(ex); }
                    //}
                }
            }
        }
    }
}