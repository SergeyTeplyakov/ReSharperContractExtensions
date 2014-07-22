using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{
    internal static class CaretUtils
    {
        public static void MoveTo(this ITextControlCaret caret, ICSharpStatement destinationStatement)
        {
            Contract.Requires(caret != null);
            Contract.Requires(destinationStatement != null);

            caret.MoveTo(destinationStatement.GetNavigationRange().TextRange.StartOffset,
                CaretVisualPlacement.Generic);
        }
    }
}