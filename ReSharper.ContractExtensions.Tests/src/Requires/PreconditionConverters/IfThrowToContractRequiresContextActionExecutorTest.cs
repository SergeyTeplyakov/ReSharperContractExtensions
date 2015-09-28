using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store.Implementation;
using JetBrains.DataFlow;
using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;
using ReSharper.ContractExtensions.Settings;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class IfThrowToContractRequiresContextActionExecutorTest
        : RequiresContextActionExecuteTestBase<IfThrowToContractRequiresContextAction>
    {
        private bool _useGenericVersion = false;

        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/PreconditionConverters"; }
        }

        protected override string ExtraPath
        {
            get { return "PreconditionConverters"; }
        }

        public override bool UseGenericVersion()
        {
            return _useGenericVersion;
        }

        [TestCase("IfThrowToContractRequiresSimple")]
        [TestCase("IfThrowToContractRequiresWithMethodCall")]
        [TestCase("IfThrowTOContractRequiresWithMessage")]
        [TestCase("IfThrowTOContractRequiresWithMessage2")]
        public void TestExecutionToNonGenericVersion(string testSrc)
        {
            UseGenerics(false);
            DoOneTest(testSrc);
        }

        [TestCase("IfThrowToContractRequiersGeneric")]
        [TestCase("IfThrowtoContractRequiresGenericAlways")]
        [TestCase("IfThrowToContractRequiresForRangeCheck")]
        public void TestExecutionToGenericVersion(string testSrc)
        {
            UseGenerics(true);
            DoOneTest(testSrc);
        }

        private void UseGenerics(bool useGenericVersion)
        {
            _useGenericVersion = useGenericVersion;
        }
    }
}