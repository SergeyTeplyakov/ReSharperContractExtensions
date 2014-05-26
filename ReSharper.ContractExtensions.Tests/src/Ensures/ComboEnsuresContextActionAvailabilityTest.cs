using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;
using ReSharper.ContractExtensions.ContextActions.Ensures;

namespace ReSharper.ContractExtensions.Tests.Postconditions
{
    [TestFixture]
    public class ComboEnsuresContextActionAvailabilityTest : CSharpContextActionAvailabilityTestBase<ComboEnsuresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "ComboEnsures"; }
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
        public void TestOtherAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }

}