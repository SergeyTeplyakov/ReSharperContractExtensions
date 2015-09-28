using JetBrains.ReSharper.FeaturesTestFramework.Intentions;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    public class ContractConverterContextActionAvailabilityTest : 
        CSharpContextActionAvailabilityTestBase<PreconditionConverterContextAction>
    {
        protected override string RelativeTestDataPath
        {
            get { return "Intentions/ContextActions/PreconditionConverters"; }
        }

        protected override string ExtraPath
        {
            get { return "PreconditionConverters"; }
        }

        [TestCase("AvailabilitySimple")]
        [TestCase("AvailabilityDebug")]
        public void TestSimpleAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

        [TestCase("AvailabilityCornerCases")]
        public void TestFullAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

    }
}