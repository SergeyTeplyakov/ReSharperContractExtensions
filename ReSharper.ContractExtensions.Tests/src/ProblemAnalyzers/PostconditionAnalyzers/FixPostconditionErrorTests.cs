using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PostconditionAnalyzers
{
    [TestFixture]
    public class FixPostconditionErrorTests : QuickFixNet45TestBase<MalformedMethodContractQuickFix>
    {
        protected override string RelativeTestDataPath
        {
            get { return @"QuickFixes\PreconditionAnalyzers\PostconditionFixes"; }
        }

        [TestCase("RemoveEnsuresForVoidReturnMethod.cs")]
        [TestCase("ChangeEnsuresForIncompatibleReturnType.cs")]
        [TestCase("ChangeEnsuresForIncompatibleReturnType2.cs")]
        [TestCase("ChangeEnsuresForIncompatibleReturnType3.cs")]
        [TestCase("ChangeEnsuresForIncompatibleReturnTypeWithProperty.cs")]
        public void Test_Quick_Fix(string fileName)
        {
            DoTestFiles(fileName);
        }
    }
}