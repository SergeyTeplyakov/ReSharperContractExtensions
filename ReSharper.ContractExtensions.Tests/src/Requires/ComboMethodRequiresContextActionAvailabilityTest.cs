using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    public class ComboMethodRequiresContextActionAvailabilityTest : 
        CSharpContextActionAvailabilityTestBase<ComboMethodRequiresContextAction>
    {
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