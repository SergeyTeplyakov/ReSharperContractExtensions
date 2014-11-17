using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Statements
{
    public static class CodeContractStatementUtils
    {
        [CanBeNull]
        public static IBlock GetTargetBlock(ICSharpStatement currentStatement)
        {
            // Looking for the block that is not a part of Try statement
            ICSharpStatement statement = currentStatement;
            while (true)
            {
                IBlock result = BlockNavigator.GetByStatement(statement);
                if (result == null)
                    return null;

                var tryStatement = result.GetContainingNode<ITryStatement>();
                if (tryStatement == null)
                    return result;

                statement = tryStatement;
            }
        }

        /// <summary>
        /// Returns a list of statements for specified function.
        /// </summary>
        public static IList<ProcessedStatement> GetLegacyContractBlockStatements(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IList<ProcessedStatement>>() != null);

            return ContractBlock.CreateLegacyContractBlock(functionDeclaration).ProcessedStatements;
        }
        
        /// <summary>
        /// Returns a list of statements for specified function.
        /// </summary>
        public static IList<ProcessedStatement> GetContractBlockStatements(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IList<ProcessedStatement>>() != null);

            return ContractBlock.Create(functionDeclaration).ProcessedStatements;
        }
        
        /// <summary>
        /// Returns a list of statements for specified function.
        /// </summary>
        public static IList<ProcessedStatement> GetCodeContractBlockStatements(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IList<ProcessedStatement>>() != null);

            return ContractBlock.CreateCodeContractBlock(functionDeclaration).ProcessedStatements;
        }

        


    }
}