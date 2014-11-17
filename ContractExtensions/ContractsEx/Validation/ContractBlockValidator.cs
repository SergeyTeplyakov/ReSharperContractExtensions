using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions.Statements;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    ///*private static bool MethodResultIsIncompatibleWith(*/)

    /// <summary>
    /// Validates code contract statementents within contract block.
    /// Class that validates <see cref="ProcessedStatement"/> into the <see cref="ValidatedContractBlock"/>.
    /// </summary>
    internal static class CodeContractBlockValidator
    {
        private static readonly List<ValidationRule> _codeContractValidationRules = GetAllValidationRules().ToList();
        private static readonly List<ValidationRule> _preconditionValidationRules = PreconditionValidator.GetValidationRules().ToList();

        /// <summary>
        ///  Validate all statements and precondition types only for legacy if-throws without EndContractBlock!
        /// </summary>
        /// <param name="contractBlock"></param>
        /// <returns></returns>
        public static ValidatedContractBlock ValidateLegacyRequires(IList<ProcessedStatement> contractBlock)
        {
            var validatedContractBlock =
                from st in contractBlock
                let vs = ValidateStatement(_preconditionValidationRules, contractBlock, st)
                select new ValidatedStatement(st, vs.ToList());
            
            return new ValidatedContractBlock(validatedContractBlock.ToList());
        }
        
        public static ValidatedContractBlock ValidateCodeContractBlock(IList<ProcessedStatement> contractBlock)
        {
            var validatedContractBlock =
                from st in contractBlock
                let vs = ValidateStatement(_codeContractValidationRules, contractBlock, st)
                select new ValidatedStatement(st, vs.ToList());
            
            return new ValidatedContractBlock(validatedContractBlock.ToList());
        }

        private static IEnumerable<ValidationRule> GetAllValidationRules()
        {
            return GetValidationRules()
                .Union(PostconditionValidator.GetValidationRules());
        }

        private static IEnumerable<ValidationRule> GetValidationRules()
        {
            yield return ValidationRule.CheckStatement(
                s =>
                {
                    // void-return methods are prohibited by CC compiler
                    if (IsVoidReturnMethod(s))
                        return ValidationResult.CreateError(s, MalformedContractError.VoidReturnMethodCall);
                    return ValidationResult.CreateNoError(s);
                });

            yield return ValidationRule.CheckStatement(
                s =>
                {
                    // Non-void return methods lead to warning from the compiler
                    if (IsNonVoidReturnMethod(s))
                        return ValidationResult.CreateWarning(s, MalformedContractWarning.NonVoidReturnMethodCall);
                    return ValidationResult.CreateNoError(s);
                });

            yield return ValidationRule.CheckStatement(
                s =>
                {
                    // Assignments are forbidden in contract block
                    if (IsAssignmentStatement(s))
                        return ValidationResult.CreateError(s, MalformedContractError.AssignmentInContractBlock);
                    return ValidationResult.CreateNoError(s);
                });

            yield return ValidationRule.CheckCodeContractStatement(
                s =>
                {
                    // Assert/Assume are forbidden in contract block
                    if ((s.StatementType == CodeContractStatementType.Assert ||
                        s.StatementType == CodeContractStatementType.Assume))
                        return ValidationResult.CreateError(s.Statement, MalformedContractError.AssertOrAssumeInContractBlock);
                    return ValidationResult.CreateNoError(s.Statement);
                });

            yield return ValidationRule.CheckContractBlock(
                (currentStatement, contractBlock) =>
                {
                    // Ensures/Ensures on throw should not be before requires
                    if (currentStatement.IsPostcondition && HasPreconditionAfterCurrentStatement(contractBlock, currentStatement))
                        return ValidationResult.CreateError(currentStatement.Statement,
                            MalformedContractError.RequiresAfterEnsures);
                    return ValidationResult.CreateNoError(currentStatement.Statement);
                });

            yield return ValidationRule.CheckContractBlock(
                (currentStatement, contractBlock) =>
                {
                    // Ensures/Ensures on throw should not be before requires
                    if (currentStatement.IsPrecondition && HasPostconditionsBeforeCurentStatement(contractBlock, currentStatement))
                        return ValidationResult.CreateError(currentStatement.Statement,
                            MalformedContractError.RequiresAfterEnsures);
                    return ValidationResult.CreateNoError(currentStatement.Statement);
                });

            yield return ValidationRule.CheckContractBlock(
                (currentStatement, contractBlock) =>
                {
                    // Ensures/Ensures on throw should not be before requires
                    if ((currentStatement.IsPrecondition || 
                         currentStatement.IsPostcondition) &&
                        HasEndContractBlockBeforeCurrentStatement(contractBlock, currentStatement))
                        return ValidationResult.CreateError(currentStatement.Statement,
                            MalformedContractError.ReqruiesOrEnsuresAfterEndContractBlock);
                    return ValidationResult.CreateNoError(currentStatement.Statement);
                });

            yield return ValidationRule.CheckContractBlock(
                (currentStatement, contractBlock) =>
                {
                    // EndContractBlock should be only one!
                    if (currentStatement.IsEndContractBlock && HasEndContractBlockBeforeCurrentStatement(contractBlock, currentStatement))
                        return ValidationResult.CreateError(currentStatement.Statement,
                            MalformedContractError.DuplicatedEndContractBlock);
                    return ValidationResult.CreateNoError(currentStatement.Statement);
                });
        }

        private static bool HasPreconditionAfterCurrentStatement(IList<ProcessedStatement> contractBlock, CodeContractStatement currentStatement)
        {
            var index = contractBlock.IndexOf(ps => ps.ContractStatement == currentStatement);
            Contract.Assert(index != -1, "Current statement should be inside contract block");

            return
                contractBlock.Skip(index + 1)
                    .Any(cs => cs.CodeContractStatement != null && cs.CodeContractStatement.IsPrecondition);
        }

        private static bool HasPostconditionsBeforeCurentStatement(IList<ProcessedStatement> contractBlock, ContractStatement currentStatement)
        {
            foreach (var c in contractBlock)
            {
                if (c.ContractStatement != null && c.ContractStatement.Equals(currentStatement))
                    return false;

                if (c.CodeContractStatement != null && c.CodeContractStatement.IsPostcondition)
                    return true;
            }

            Contract.Assert(false, "Current statement not found in the contract block");
            throw new InvalidOperationException("Current statement not found in the contract block");
        }

        private static bool HasEndContractBlockBeforeCurrentStatement(IList<ProcessedStatement> contractBlock, ContractStatement currentStatement)
        {
            foreach (var c in contractBlock)
            {
                if (c.ContractStatement != null && c.ContractStatement.Equals(currentStatement))
                    return false;

                if (c.CodeContractStatement != null && 
                    c.CodeContractStatement.StatementType == CodeContractStatementType.EndContractBlock)
                    return true;
            }

            Contract.Assert(false, "Current statement not found in the contract block");
            throw new InvalidOperationException("Current statement not found in the contract block");
        }

        private static IEnumerable<ValidationResult> ValidateStatement(IList<ValidationRule> validationRules, 
            IList<ProcessedStatement> contractBlock, ProcessedStatement currentStatement)
        {
            Contract.Requires(contractBlock != null);
            Contract.Requires(currentStatement != null);
            Contract.Ensures(Contract.Result<IEnumerable<ValidationResult>>() != null);

            return validationRules
                .Select(rule => rule.Validate(currentStatement, contractBlock))
                .Where(vr => vr.ErrorType != ErrorType.NoError)
                .ToList();
        }

        private static bool IsAssignmentStatement(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            return statement.With(x => x as IExpressionStatement)
                    .With(x => x.Expression as IAssignmentExpression) != null;
        }


        private static bool IsVoidReturnMethod(ICSharpStatement statement)
        {
            return
                statement.GetInvokedMethod()
                    .ReturnStruct(x => x.ReturnType.IsVoid()) == true;
        }

        private static bool IsNonVoidReturnMethod(ICSharpStatement statement)
        {
            return
                statement.GetInvokedMethod()
                    .ReturnStruct(x => x.ReturnType.IsVoid()) == false;
        }
    }
}