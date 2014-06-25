using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    internal sealed class ComboEnsuresExecutor : ContextActionExecutorBase
    {
        private readonly AddContractAvailability _addContractAvailability;
        private readonly ICSharpFunctionDeclaration _selectedFunctionDeclaration;

        public ComboEnsuresExecutor(ICSharpContextActionDataProvider provider, 
            AddContractAvailability addContractAvailability,
            ICSharpFunctionDeclaration selectedFunctionDeclaration)
            : base(provider)
        {
            Contract.Requires(provider != null);
            Contract.Requires(addContractAvailability != null);
            Contract.Requires(addContractAvailability.IsAvailable);

            Contract.Requires(selectedFunctionDeclaration != null);

            _addContractAvailability = addContractAvailability;
            _selectedFunctionDeclaration = selectedFunctionDeclaration;
        }

        public override void ExecuteTransaction()
        {
            var addContractExecutor = new AddContractExecutor(_provider, _addContractAvailability, 
                _selectedFunctionDeclaration);
            addContractExecutor.Execute();

            var addEnsuresExecutor = new EnsuresExecutor(_provider, _selectedFunctionDeclaration);
            addEnsuresExecutor.ExecuteTransaction();
        }
    }
}