using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Statements
{
    public sealed class ProcessedStatement
    {
        public ICSharpStatement CSharpStatement { get; private set; }
        public CodeContractStatement ContractStatement { get; private set; }

        public static ProcessedStatement Create(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);
            Contract.Ensures(Contract.Result<ProcessedStatement>() != null);

            return new ProcessedStatement
            {
                CSharpStatement = statement,
                ContractStatement = CodeContractStatement.TryCreate(statement),
            };
        }
    }

    public static class CodeContractStatementUtils
    {
        /// <summary>
        /// Returns a list of statements for specified function.
        /// </summary>
        public static IList<ProcessedStatement> GetContractBlockStatements(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IList<ProcessedStatement>>() != null);

            var statements =
                GetStatements(functionDeclaration.Body)
                .Select(ProcessedStatement.Create)
                .ToList();

            int lastContractIndex =
                statements.LastIndexOf(s => s.ContractStatement != null &&
                            (s.ContractStatement.StatementType == CodeContractStatementType.Requires ||
                             s.ContractStatement.StatementType == CodeContractStatementType.Ensures ||
                             s.ContractStatement.StatementType == CodeContractStatementType.EndContractBlock));

            // Because we're taking +1 item we can skip check for -1!
            return statements.Take(lastContractIndex + 1).ToList();
        }

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

                return new [] {s}.AsEnumerable();
            });
        }

    }
}