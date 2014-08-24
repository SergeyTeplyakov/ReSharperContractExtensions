using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;

namespace ReSharper.ContractExtensions.Tests.ContractFor
{
    [TestFixture]
    public class AddContractForContextActionAvailabilityTest : CSharpContextActionAvailabilityTestBase<AddContractClassContextAction>
    {
        protected override string ExtraPath
        {
            get { return "ContractFor"; }
        }

        [TestCase("01_AvailableOnInterface")]
        [TestCase("015_AvailableOnTheMiddleOfInterface")]
        [TestCase("02_AvailableOnAbstractClass")]
        [TestCase("025_AvailableOnTheMiddleOfAbstractClass")]
        [TestCase("03_UnavailableOnInterface")]
        [TestCase("04_UnavailableOnAbstractClass")]
        [TestCase("05_UnavailableOnConcreteClass")]
        [TestCase("06_AvailableOnPartiallyDefinedContractForInterface")]
        [TestCase("07_AvailableOnPartiallyDefinedContractForClass")]
        [TestCase("08_UnavailableInsideInterface")]
        [TestCase("09_UnavailableInsideAbstractClass")]
        public void TestSimpleAvailability(string testSrc)
        {
            DoOneTest(testSrc);
        }

    }
}