using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Statements
{
    /// <summary>
    /// Represetns block of processed statements belong to contract section of the method.
    /// </summary>
    public sealed class ContractBlock
    {
        private readonly IList<ProcessedStatement> _processedStatements;

        private ContractBlock(IList<ProcessedStatement> processedStatements)
        {
            Contract.Requires(processedStatements != null);

            _processedStatements = processedStatements;
        }

        public IList<ProcessedStatement> ProcessedStatements
        {
            get
            {
                Contract.Ensures(Contract.Result<IList<ProcessedStatement>>() != null);
                return _processedStatements;
            }
        }

        public static ContractBlock Create(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<ContractBlock>() != null);

            var statements =
                GetStatements(functionDeclaration.Body)
                .Select(ProcessedStatement.Create)
                .ToList();

            int lastContractIndex =
                statements.LastIndexOf(
                    s => (s.ContractStatement != null && s.ContractStatement.IsPrecondition) ||
                         (s.CodeContractStatement != null &&
                         (s.CodeContractStatement.IsPrecondition ||
                          s.CodeContractStatement.IsPostcondition ||
                          s.CodeContractStatement.StatementType == CodeContractStatementType.EndContractBlock)));

            // Because we're taking +1 item we can skip check for -1!
            return new ContractBlock(statements.Take(lastContractIndex + 1).ToList());
        }
        
        public static ContractBlock CreateLegacyContractBlock(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<ContractBlock>() != null);

            var statements =
                GetStatements(functionDeclaration.Body)
                .Select(ProcessedStatement.Create)
                .Where(x => x.ContractStatement != null)
                .ToList();

            if (statements.All(s => s.ContractStatement.IsIfThrowStatement()))
                return new ContractBlock(statements);
            
            return new ContractBlock(new ProcessedStatement[]{});
        }
        
        /// <summary>
        /// Code contract statement contains only a list of Code Contract statements.
        /// </summary>
        public static ContractBlock CreateCodeContractBlock(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<ContractBlock>() != null);

            var statements =
                GetStatements(functionDeclaration.Body)
                .Select(ProcessedStatement.Create)
                .ToList();

            int lastContractIndex =
                statements.LastIndexOf(
                    s => (s.CodeContractStatement != null &&
                         (s.CodeContractStatement.IsPrecondition ||
                          s.CodeContractStatement.IsPostcondition ||
                          s.CodeContractStatement.StatementType == CodeContractStatementType.EndContractBlock)));

            // Because we're taking +1 item we can skip check for -1!
            return new ContractBlock(statements.Take(lastContractIndex + 1).ToList());
        }

        /// <summary>
        /// Return statements for specified <see cref="block"/> and "sub-blocks" recursively.
        /// </summary>
        /// <remarks>
        /// Please note, that this function only returns inner "pure" bocks, not any blocks
        /// that are resides inside other statements like if, switch, try etc.
        /// </remarks>
        private static IEnumerable<ICSharpStatement> GetStatements(IBlock block)
        {
            Contract.Ensures(Contract.Result<IEnumerable<ICSharpStatement>>() != null);

            if (block == null)
                return Enumerable.Empty<ICSharpStatement>();

            return block.Statements.SelectMany(s =>
            {
                var innerBlock = s as IBlock;

                if (innerBlock != null)
                    return GetStatements(innerBlock);

                return new[] { s }.AsEnumerable();
            });
        }

    }
}