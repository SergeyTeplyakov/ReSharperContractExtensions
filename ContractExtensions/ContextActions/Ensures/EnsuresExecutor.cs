using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    internal sealed class EnsuresExecutor : ContextActionExecutorBase
    {
        private readonly ICSharpFunctionDeclaration _selectedFunction;

        public EnsuresExecutor(ICSharpContextActionDataProvider provider, ICSharpFunctionDeclaration selectedFunction)
            : base(provider)
        {
            Contract.Requires(provider != null);
            Contract.Requires(selectedFunction != null);

            _selectedFunction = selectedFunction;
        }

        public override void ExecuteTransaction()
        {
            var functionDeclaration = _selectedFunction;

            var contractFunction = functionDeclaration.GetContractFunction();
            Contract.Assert(contractFunction != null);

            var ensureStatement = CreateContractEnsures(_factory, contractFunction);

            var requires = GetLastRequiresStatementIfAny(contractFunction);
            AddStatementAfter(contractFunction, ensureStatement, requires);
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

            string format = "$0.Ensures(Contract.Result<$1>() != null);";
            var returnType = (IDeclaredType)functionDeclaration.DeclaredElement.ReturnType;

            return factory.CreateStatement(format, ContractType, returnType);
        }

        private ICSharpStatement GetLastRequiresStatementIfAny(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.GetContractPreconditions()
                .Select(r => r.Statement).LastOrDefault();
        }

        private void AddStatementAfter(ICSharpFunctionDeclaration functionDeclaration,
            ICSharpStatement statement, ICSharpStatement anchor)
        {
            functionDeclaration.Body.AddStatementAfter(statement, anchor);
        }
    }
}