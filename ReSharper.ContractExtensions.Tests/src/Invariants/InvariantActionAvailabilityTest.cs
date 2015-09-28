using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;
using ReSharper.ContractExtensions.ContextActions.Invariants;

namespace ReSharper.ContractExtensions.Tests.Invariants
{
    [TestFixture]
    public class AddObjectInvariantTest : CSharpContextActionAvailabilityTestBase<InvariantContextAction>
    {
        protected override string RelativeTestDataPath { get { return "Intentions/ContextActions/Invariants"; } }

        protected override string ExtraPath
        {
            get { return "Invariants"; }
        }

        [TestCase("Availability")]
        [TestCase("AvailabilityDebug")]
        public void TestSimpleAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

        [TestCase("AvailabilityFull")]
        [TestCase("AvailabilityOnStruct")]
        [TestCase("AvailabilityOnAbstractClass")]
        public void TestAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}