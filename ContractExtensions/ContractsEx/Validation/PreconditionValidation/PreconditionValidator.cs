using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions.Statements;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    /// <summary>
    /// In some cases precondition's are redundant or inappropriate. For instance, public method 
    /// should either have a preconditions or be async. Otherwise exception (because of violated precondition)
    /// would be raised during task consumtion but not during task creation.
    /// </summary>
    internal static class PreconditionValidator
    {
        public static IEnumerable<ValidationRule> GetValidationRules()
        {
            yield return ValidationRule.CheckContractStatement(
                s =>
                {
                    if (s.IsPrecondition && IsContainingMethodAsync(s.Statement))
                        return ValidationResult.CreateCustomWarning(s.Statement, 
                            MalformedContractCustomWarning.PreconditionInAsyncMethod);
                    return ValidationResult.CreateNoError(s.Statement);
                });

            yield return ValidationRule.CheckContractStatement(
                s =>
                {
                    if (s.IsPrecondition && IsContainingMethodContainsIteratorBlock(s.Statement))
                        return ValidationResult.CreateCustomWarning(s.Statement, 
                            MalformedContractCustomWarning.PreconditionInMethodWithIteratorBlock);
                    return ValidationResult.CreateNoError(s.Statement);
                });
        }

        private static bool IsContainingMethodContainsIteratorBlock(ICSharpStatement statement)
        {
            return statement
                .GetContainingTypeMemberDeclaration()
                .With(x => x as ICSharpFunctionDeclaration)
                .ReturnStruct(x => x.IsIterator) == true;
        }

        private static bool IsContainingMethodAsync(ICSharpStatement statement)
        {
            return statement
                .GetContainingTypeMemberDeclaration()
                .With(x => x as ICSharpFunctionDeclaration)
                .ReturnStruct(x => x.IsAsync) == true;
        }
    }
}