using System;
using System.Diagnostics.Contracts;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    public enum MalformedContractError
    {
        VoidReturnMethodCall,
        AssertOrAssumeInContractBlock,
        AssignmentInContractBlock,
        RequiresAfterEnsures,
        ReqruiesOrEnsuresAfterEndContractBlock,
        DuplicatedEndContractBlock,
    }

    public enum MalformedContractWarning
    {
        NonVoidReturnMethodCall,
    }

    public static class MalformedContractErrorEx
    {
        public static string GetErrorText(this MalformedContractError error, string contractMethodName)
        {
            switch (error)
            {
                case MalformedContractError.VoidReturnMethodCall:
                    // Code Contract error: error CC1027: Malformed contract.
                    return string.Format("Malformed contract. Detected expression statement evaluated for side-effect in contracts of method '{0}'",
                        contractMethodName);

                case MalformedContractError.AssertOrAssumeInContractBlock:
                    // Code Contract error:  error CC1016: Contract.Assert/Contract.Assume cannot be used in contract section. Use only Requires and Ensures.
                    return string.Format("Contract.Assert/Contract.Assume cannot be used in contract section of method '{0}'. Use only Requires and Ensures",
                        contractMethodName);

                case MalformedContractError.AssignmentInContractBlock:
                    // Code Contract error: error CC1004: Malformed contract. Found Requires after assignment in method 'CodeContractInvestigations.InconsistentPreconditionVisibility.AssignmentBeforeRequires'.
                    return string.Format("Malformed contract. Assignment cannot be used in contract section of method '{0}'",
                        contractMethodName);

                case MalformedContractError.RequiresAfterEnsures:
                    // error CC1014: Precondition found after postcondition.
                    return string.Format("Malformed contract. Precondition found after postcondition in contract section of method '{0}'",
                        contractMethodName);

                case MalformedContractError.ReqruiesOrEnsuresAfterEndContractBlock:
                    // error CC1012: Contract call found after prior EndContractBlock.
                    return string.Format("Malformed contract. Contract call found after prior EndContractBlock in method '{0}'",
                        contractMethodName);

                case MalformedContractError.DuplicatedEndContractBlock:
                    //error CC1012: Contract call found after prior EndContractBlock.
                    return string.Format("Malformed contract. Duplicated call of EndContractBlock in method '{0}'",
                        contractMethodName);

                default:
                    Contract.Assert(false, "Unknown malformed contract error: " + error);
                    throw new InvalidOperationException("Unknown malformed contract error: " + error);
            }

            //warning CC1069: Detected expression statement evaluated for potential side-effect in contracts of method 'RequiresInconsistentVisibility.WarningForMalformedContract.WarningOnMethodCallWithResul'.
        }
    }

    public static class MalformedContractWarningEx
    {
        public static string GetErrorText(this MalformedContractWarning warning, string contractMethodName)
        {
            switch (warning)
            {
                case MalformedContractWarning.NonVoidReturnMethodCall:
                    //Code Contract warning: warning CC1069: Detected expression statement evaluated for potential side-effect in contracts of method 'RequiresInconsistentVisibility.WarningForMalformedContract.WarningOnMethodCallWithResul'.
                    return string.Format("Detected expression statement evaluated for potential side-effect in contracts of method '{0}'",
                        contractMethodName);
                default:
                    Contract.Assert(false, "Unknown malformed contract warning: " + warning);
                    throw new InvalidOperationException("Unknown malformed contract warning: " + warning);
            }

            //warning CC1069: Detected expression statement evaluated for potential side-effect in contracts of method 'RequiresInconsistentVisibility.WarningForMalformedContract.WarningOnMethodCallWithResul'.
        }
    }

}