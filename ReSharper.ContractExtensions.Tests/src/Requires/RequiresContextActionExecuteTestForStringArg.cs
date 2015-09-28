using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store.Implementation;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using JetBrains.ReSharper.Resources.Shell;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;
using ReSharper.ContractExtensions.Settings;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class RequiresContextActionExecuteTestForStringArg : CSharpContextActionExecuteTestBase<AddRequiresContextAction>
    {
        protected override void DoTest(IProject testProject)
        {
            Lifetimes.Using(lifetime =>
            {
                ChangeSettingsTemporarily(lifetime);


                var settingsStore = Shell.Instance.GetComponent<SettingsStore>();
                var context = ContextRange.ManuallyRestrictWritesToOneContext((_, contexts) => contexts.Empty);
                var settings = settingsStore.BindToContextTransient(context);

                settings.SetValue((ContractExtensionsSettings s) => s.CheckStringsForNullOrEmpty, true);

                base.DoTest(testProject);
            });
        }

        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/RequiresWithString"; }
        }

        protected override string ExtraPath
        {
            get { return "RequiresWithString"; }
        }

        [TestCase("ExecutionForStringArg")]
        public void TestSimpleExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}