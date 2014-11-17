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
        MethodContractInTryBlock,
        ContractStatementInTheMiddleOfTheMethod,

        // Postcondition errors
        ResultTypeInEnsuresIsIncompatibleWithMethodReturnType,
        EnsuresInVoidReturnMethod,
    }

    public enum MalformedContractWarning
    {
        NonVoidReturnMethodCall,
    }

    public enum MalformedContractCustomWarning
    {
        PreconditionInAsyncMethod,
        PreconditionInMethodWithIteratorBlock,
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

                case MalformedContractError.MethodContractInTryBlock:
                    // error CC1024: Contract section within try block.
                    return string.Format("Malformed contract. Contract section within try block in method '{0}'",
                        contractMethodName);
                case MalformedContractError.ContractStatementInTheMiddleOfTheMethod:
                    // error CC1017: Malformed contract section in method 'MalformedContractErrors.ContractInTheMiddleOfTheMethod(System.String)'
                    return string.Format("Contract statements in the middle of the method '{0}'",
                        contractMethodName);
                default:
                    Contract.Assert(false, "Unknown malformed contract error: " + error);
                    throw new InvalidOperationException("Unknown malformed contract error: " + error);
            }
        }

        //public static string ErrorForIncompatibleEnsuresAndReturnType(IType methodResult, IDeclaredType contractResult,
        //    ICSharpFunctionDeclaration method)
        //{
        //    string kind = "method";
        //    string name = method.DeclaredName;

        //    var property = method as IAccessorDeclaration;
        //    if (property != null)
        //    {
        //        kind = "property";
        //        name = name.Replace("get_", "");
        //    }

        //    return string.Format("Detected a call to Result with '{0}' in {1} '{2}', should be '{3}'",
        //        contractResult.GetPresentableName(CSharpLanguage.Instance), kind, name,
        //        methodResult.GetPresentableName(CSharpLanguage.Instance));
        //}

        public static string GetErrorText(this MalformedContractCustomWarning warning, string contractMethodName)
        {
            switch (warning)
            {
                case MalformedContractCustomWarning.PreconditionInAsyncMethod:
                    return string.Format("Lecacy precondition in async method is asynchronous and will fail returning task");
                case MalformedContractCustomWarning.PreconditionInMethodWithIteratorBlock:
                    return string.Format("Legacy precondition in iterator block will throw only on first MoveNext() call");
                default:
                    Contract.Assert(false, "Unknown custom warning: " + warning);
                    throw new InvalidOperationException("Unknown custom warning: " + warning);
            }
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
        }
    }

}