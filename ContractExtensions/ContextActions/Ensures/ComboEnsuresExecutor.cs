using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    internal sealed class ComboEnsuresExecutor
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly AddContractAvailability _addContractAvailability;
        private readonly ICSharpFunctionDeclaration _selectedFunctionDeclaration;

        public ComboEnsuresExecutor(ICSharpContextActionDataProvider provider, 
            AddContractAvailability addContractAvailability,
            ICSharpFunctionDeclaration selectedFunctionDeclaration)
        {
            Contract.Requires(provider != null);
            Contract.Requires(addContractAvailability != null);
            Contract.Requires(addContractAvailability.IsAvailable);

            Contract.Requires(selectedFunctionDeclaration != null);

            _provider = provider;
            _addContractAvailability = addContractAvailability;
            _selectedFunctionDeclaration = selectedFunctionDeclaration;
        }

        public void Execute(ISolution solution, IProgressIndicator progress)
        {
            var addContractExecutor = new AddContractExecutor(_provider, _addContractAvailability);
            addContractExecutor.Execute(solution, progress);

            var addEnsuresExecutor = new EnsuresExecutor(_provider, _selectedFunctionDeclaration);
            addEnsuresExecutor.Execute(solution, progress);
        }
    }
}