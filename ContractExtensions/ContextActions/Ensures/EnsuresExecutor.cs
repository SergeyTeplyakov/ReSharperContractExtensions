using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    internal sealed class EnsuresExecutor
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly EnsuresAvailability _availability;
        private readonly ICSharpFunctionDeclaration _selectedFunction;


        public EnsuresExecutor(ICSharpContextActionDataProvider provider, EnsuresAvailability availability)
        {
            Contract.Requires(provider != null);
            Contract.Requires(availability != null);
            Contract.Requires(availability.IsAvailable);

            _provider = provider;
            _availability = availability;

            _selectedFunction = _availability.SelectedFunction;
        }

        public void ExecuteTransaction(ISolution solution, IProgressIndicator progress)
        {
            var functionDeclaration = _selectedFunction;

            var contractFunction = functionDeclaration.GetContractFunction();

            var factory = _provider.ElementFactory;

            ICSharpFile currentFile = (ICSharpFile)functionDeclaration.GetContainingFile();
            Contract.Assert(currentFile != null);

            AddNamespaceUsing(factory, currentFile);
            var ensureStatement = CreateContractEnsures(factory, contractFunction);

            var requires = GetLastRequiresStatementIfAny(contractFunction);
            AddStatementAfter(contractFunction, ensureStatement, requires);
        }

        private static void AddNamespaceUsing(CSharpElementFactory factory, ICSharpFile currentFile)
        {
            // TODO: how to express this in postcondition? Add another helper method for this!!
            var diagnostics = factory.CreateUsingDirective("using $0", typeof(Contract).Namespace);
            if (!currentFile.Imports.ContainsUsing(diagnostics))
            {
                currentFile.AddImport(diagnostics);
            }
        }

        private ICSharpStatement CreateContractEnsures(CSharpElementFactory factory, ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(factory != null);
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);
            // TODO: IsValid returns true even if resulting expression would not compiles!
            // To check this, please remove semicolon in the string below.
            // Maybe Resolve method would be good!
            // Contract.Ensures(Contract.Result<ICSharpStatement>().IsValid());

            string statementLiteral = string.Format("{0}.Ensures(Contract.Result<{1}>() != null);",
                typeof(Contract).Name,
                functionDeclaration.DeclaredElement.ReturnType.GetPresentableName(CSharpLanguage.Instance));
            var result = factory.CreateStatement(statementLiteral);

            return result;
        }

        private ICSharpStatement GetLastRequiresStatementIfAny(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.GetRequires().Select(r => r.Statement).LastOrDefault();
        }

        private void AddStatementAfter(ICSharpFunctionDeclaration functionDeclaration,
            ICSharpStatement statement, ICSharpStatement anchor)
        {
            functionDeclaration.Body.AddStatementAfter(statement, anchor);
        }
    }
}