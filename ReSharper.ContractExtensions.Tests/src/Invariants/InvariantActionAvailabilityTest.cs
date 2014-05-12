using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;

namespace ReSharper.ContractExtensions.Tests.Invariants
{
    [TestFixture]
    public class AddObjectInvariantTest : CSharpContextActionAvailabilityTestBase<InvariantContextAction>
    {
        protected override string ExtraPath
        {
            get { return "Invariants"; }
        }

        [TestCase("Availability")]
        public void TestSimpleAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

        [TestCase("AvailabilityFull")]
        [TestCase("AvailabilityOnStruct")]
        public void TestAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}