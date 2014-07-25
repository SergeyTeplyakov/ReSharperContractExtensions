using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Statements;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal static class ContractStatementValidator
    {
        private static readonly List<SingleStatementValidationRule> _validationRules = GetValidationRules().ToList();

        public static ValidationResult ValidateStatement(CodeContractStatement codeContractStatement)
        {
            Contract.Requires(codeContractStatement != null);
            Contract.Ensures(Contract.Result<ValidationResult>() != null);

            return _validationRules
                .Select(vr => vr.Validate(codeContractStatement))
                .FirstOrDefault(vr => vr.ErrorType != ErrorType.NoError) ?? ValidationResult.CreateNoError(codeContractStatement.Statement);
        }

        private static IEnumerable<SingleStatementValidationRule> GetValidationRules()
        {
            yield return SingleStatementValidationRule.Create(
                s =>
                {
                    // Assert/Assume are forbidden in contract block
                    if (s.IsMethodContractStatement && InsideInnerStatement(s.Statement))
                        return ValidationResult.CreateError(s.Statement, MalformedContractError.ContractStatementInTheMiddleOfTheMethod);
                    return ValidationResult.CreateNoError(s.Statement);
                });

        }

        private static bool InsideInnerStatement(ICSharpStatement statement)
        {
            return statement.IsInside<IIfStatement>() ||

                   statement.IsInside<ILoopStatement>() ||
                   statement.IsInside<ISwitchStatement>() ||
                   IsInCaseBlock(statement) ||

                   statement.IsInside<ICatchClause>() ||
                   IsInFinally(statement) ||
                   
                   statement.IsInside<IUsingStatement>();
        }

        private static bool IsInFinally(ICSharpStatement statement)
        {
            var block = BlockNavigator.GetByStatement(statement);

            while (block != null)
            {
                if (block.Parent is ITryStatement)
                    return true;

                block = block.Parent as IBlock;
            }

            return false;
        }

        private static bool IsInCaseBlock(ICSharpStatement statement)
        {
            var block = BlockNavigator.GetByStatement(statement);

            while (block != null)
            {
                if (block.Parent is ISwitchStatement)
                    return true;

                block = block.Parent as IBlock;
            }

            return false;
        }
    }

    static class StatementExtensions
    {
        public static bool IsInside<T>(this ICSharpStatement statement) where T : ITreeNode
        {
            Contract.Requires(statement != null);

            return statement.GetContainingNode<T>() != null;
        }
    }
}