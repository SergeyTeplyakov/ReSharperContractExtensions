using System.Diagnostics.Contracts;
using System.Reflection;
using JetBrains.ReSharper.Intentions.CSharp.Test;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.EnumChecks;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class EnumCheckEnsuresContextActionAvailabilityTest : CSharpContextActionAvailabilityTestBase<EnumCheckEnsuresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "EnumCheckEnsures"; }
        }

        [TestCase("Availability")]
        public void TestSimpleAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

        //[TestCase("AvailabilityCornerCases")]
        //[TestCase("AvailabilityOnPropertySetter")]
        //public void Test_Corner_Cases(string testSrc)
        //{
        //    DoOneTest(testSrc);
        //}

        //[TestCase("AvailabilityFull")]
        //[TestCase("AvailabilityOnAbstractClass")]
        //[TestCase("AvailabilityOnInterface")]
        //[TestCase("AvailabilityOnStaticClass")]
        //public void TestFullAvailability(string testSrc)
        //{
        //    DoOneTest(testSrc);
        //}
    }
}