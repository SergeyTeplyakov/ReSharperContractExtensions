using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class GenericContractRequiresToContractRequiresContextActionExecutorTest
        : CSharpContextActionExecuteTestBase<GenericContractRequiresToContractRequiresContextAction>
    {
        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/PreconditionConverters"; }
        }
        protected override string ExtraPath
        {
            get { return "PreconditionConverters"; }
        }

        [TestCase("GenericContractRequiresToNonGenericSimple")]
        public void TestExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}