using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store.Implementation;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.Settings;
using ContextRange = JetBrains.Application.Settings.ContextRange;

namespace ReSharper.ContractExtensions.Tests.ContractFor
{
    [TestFixture]
    public class AddContractForWIthExcludeCodeCoverageTest : CSharpContextActionExecuteTestBase<AddContractContextAction>
    {
        protected override string ExtraPath
        {
            get { return "ContractFor"; }
        }

        [TestCase("ExecutionWithExcludeFromCodeCoverage")]
        public void TestExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }

        protected override void DoTest(IProject testProject)
        {
            Lifetimes.Using(lifetime =>
            {
                ChangeSettingsTemporarily(lifetime);


                var settingsStore = Shell.Instance.GetComponent<SettingsStore>();
                var context = ContextRange.ManuallyRestrictWritesToOneContext((_, contexts) => contexts.Empty);
                var settings = settingsStore.BindToContextTransient(context);

                settings.SetValue((ContractExtensionsSettings s) => s.UseExcludeFromCodeCoverageAttribute, true);

                base.DoTest(testProject);
            });
        }

    }
}