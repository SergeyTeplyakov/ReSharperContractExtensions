using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    public class ContractConverterContextActionAvailabilityTest : 
        CSharpContextActionAvailabilityTestBase<PreconditionConverterContextAction>
    {
        protected override string ExtraPath
        {
            get { return "PreconditionConverters"; }
        }

        [TestCase("AvailabilitySimple")]
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