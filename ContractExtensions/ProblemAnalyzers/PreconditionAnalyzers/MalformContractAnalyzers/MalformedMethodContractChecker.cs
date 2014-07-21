using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Statements;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers
{

    /// <summary>
    /// Warns if method contract is malformed:
    /// - Contract statements are not the first statements in the method
    /// - Ensures statement is after precondition check
    /// </summary>
    [ElementProblemAnalyzer(new[] { typeof(ICSharpFunctionDeclaration) },
    HighlightingTypes = new[] { typeof(MalformedMethodContractErrorHighlighting), typeof(MalformedMethodContractWarningHighlighting) })]
    public sealed class MalformedMethodContractChecker : ElementProblemAnalyzer<ICSharpFunctionDeclaration>
    {
        protected override void Run(ICSharpFunctionDeclaration element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            var contractBlockStatements = element.GetContractBlockStatements();

            string methodName = element.DeclaredName;

            foreach (var vr in ContractBlockValidator.ValidateContractBlockStatements(contractBlockStatements)
                        .Where(v => v.ErrorType != ErrorType.NoError))
            {
                if (vr.ErrorType == ErrorType.CodeContractError)
                {
                    consumer.AddHighlighting(
                        new MalformedMethodContractErrorHighlighting(vr.MalformedContractError, methodName, element, contractBlockStatements, vr.Statement),
                        vr.Statement.GetDocumentRange(), element.GetContainingFile());
                }
                else if (vr.ErrorType == ErrorType.CodeContractWarning)
                {
                    consumer.AddHighlighting(
                        new MalformedMethodContractWarningHighlighting(vr.MalformedContractWarning, methodName, element, contractBlockStatements, vr.Statement), 
                        vr.Statement.GetDocumentRange(), element.GetContainingFile());
                }
            }
        }
    }
}