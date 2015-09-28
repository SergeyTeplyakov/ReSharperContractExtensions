using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    public class ComboRequiresContextActionAvailabilityTest : 
        CSharpContextActionAvailabilityTestBase<ComboRequiresContextAction>
    {
        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/ComboRequires"; }
        }
        protected override string ExtraPath
        {
            get { return "ComboRequires"; }
        }

        [TestCase("Availability")]
        public void TestSimpleAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

        [TestCase("AvailabilityForAbstractClass")]
        [TestCase("AvailabilityForInterface")]
        [TestCase("AvailabilityWhenContractClassExists")]
        [TestCase("AvailabilityWhenContractClassForInterfaceExists")]
        public void TestFullAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

    }
}