using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class ContractRequiresToGenericContractRequiresContextActionExecutorTest 
        : CSharpContextActionExecuteTestBase<ContractRequiresToGenericContractRequiresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "PreconditionConverters"; }
        }

        [TestCase("ContractRequiresToGenericSimple")]
        [TestCase("ContractRequiresToGenericComplexCheck")]
        [TestCase("ContractRequiresToGenericWithMethodCall")]
        [TestCase("ContractRequiresToGenericWithMessage")]
        [TestCase("ContractRequiresToGenericWithArgumentException")]
        public void TestExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}