using JetBrains.ReSharper.Intentions.CSharp.Test;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.Tests.Preconditions
{
    [TestFixture]
    public class GenericContractRequiresToContractRequiresContextActionExecutorTest
        : CSharpContextActionExecuteTestBase<GenericContractRequiresToContractRequiresContextAction>
    {
        protected override string ExtraPath
        {
            get { return "PreconditionConverters"; }
        }

        [TestCase("GenericContractRequiresToNonGenericSimple")]
        public void TestExecution(string testSrc)
        {
            DoOneTest(testSrc);
        }
    }
}