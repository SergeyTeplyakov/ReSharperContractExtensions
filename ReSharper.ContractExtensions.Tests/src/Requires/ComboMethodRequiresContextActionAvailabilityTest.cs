using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    public class ComboMethodRequiresContextActionAvailabilityTest : 
        CSharpContextActionAvailabilityTestBase<ComboMethodRequiresContextAction>
    {
        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/ComboRequiresForMethod"; }
        }

        protected override string ExtraPath
        {
            get { return "ComboRequiresForMethod"; }
        }

        [TestCase("AvailabilitySimple")]
        public void TestSimpleAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

        [TestCase("AvailabilityForAbstractClass")]
        [TestCase("AvailabilityForInterface")]
        public void TestFullAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

    }
}