using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal static class StatementUtils
    {
        public static void DetachFromParent(this ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var parentBlock = BlockNavigator.GetByStatement(statement);
            Contract.Assert(parentBlock != null, "Can't find parent for the statement!");

            parentBlock.RemoveStatement(statement);
        }

        public static ICSharpStatement AddStatementsTo(this IBlock block, IEnumerable<ICSharpStatement> statements, ICSharpStatement selectedStatement)
        {
            Contract.Requires(block != null);
            Contract.Requires(statements != null);
            Contract.Requires(selectedStatement != null);

            ICSharpStatement result = null;
            ICSharpStatement localAnchor = null;
            foreach (var s in statements)
            {
                localAnchor = block.AddStatementAfter(s, localAnchor);

                if (s == selectedStatement)
                    result = localAnchor;
            }

            return result;

        }


        /// <summary>
        /// Method will add specified <paramref name="statements"/> to the <paramref name="anchor"/>.
        /// </summary>
        /// <remarks>
        /// Adding statement is pure and new statement will be cloned and attached to the parent.
        /// In some cases crusial to get back some statement in the new tree. In this case return value and 
        /// selected statement could be used.
        /// </remarks>
        public static ICSharpStatement AddStatementsAfter(this ICSharpStatement anchor, IEnumerable<ICSharpStatement> statements, 
            ICSharpStatement selectedStatement)
        {
            Contract.Requires(anchor != null);
            Contract.Requires(statements != null);
            Contract.Requires(selectedStatement != null);

            var localAnchor = anchor;
            var parent = BlockNavigator.GetByStatement(localAnchor);
            Contract.Assert(parent != null, "Can't find parent for the last contract statement.");
            ICSharpStatement result = null;

            foreach (var s in statements)
            {
                localAnchor = parent.AddStatementAfter(s, localAnchor);

                if (s == selectedStatement)
                    result = localAnchor;
            }

            return result;
        }

        public static ICSharpStatement AddStatementsBefore(this ICSharpStatement anchor, IEnumerable<ICSharpStatement> statements, 
            ICSharpStatement selectedStatement)
        {
            Contract.Requires(anchor != null);
            Contract.Requires(statements != null);
            Contract.Requires(selectedStatement != null);

            var localAnchor = anchor;
            var parent = BlockNavigator.GetByStatement(localAnchor);
            Contract.Assert(parent != null, "Can't find parent for the last contract statement.");
            ICSharpStatement result = null;

            foreach (var s in statements)
            {
                localAnchor = parent.AddStatementBefore(s, localAnchor);

                if (s == selectedStatement)
                    result = localAnchor;
            }

            return result;
        }
    }
}