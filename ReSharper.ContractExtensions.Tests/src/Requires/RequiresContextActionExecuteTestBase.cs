using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store.Implementation;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Intentions.CSharp.Test;
using JetBrains.ReSharper.Intentions.Extensibility;
using ReSharper.ContractExtensions.Settings;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    public abstract class RequiresContextActionExecuteTestBase<TContextAction> 
        : CSharpContextActionExecuteTestBase<TContextAction> where TContextAction : class, IContextAction
    {
        public virtual bool UseGenericVersion()
        {
            return false;
        }

        protected override void DoTest(IProject testProject)
        {
            Lifetimes.Using(lifetime =>
            {
                ChangeSettingsTemporarily(lifetime);


                var settingsStore = Shell.Instance.GetComponent<SettingsStore>();
                var context = ContextRange.ManuallyRestrictWritesToOneContext((_, contexts) => contexts.Empty);
                var settings = settingsStore.BindToContextTransient(context);

                settings.SetValue((ContractExtensionsSettings s) => s.UseGenericContractRequires, UseGenericVersion());
                settings.SetValue((ContractExtensionsSettings s) => s.CheckStringsForNullOrEmpty, false);

                base.DoTest(testProject);
            });
        }

    }
}