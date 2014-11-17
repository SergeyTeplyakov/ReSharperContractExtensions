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
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly AddContractClassAvailability _addContractAvailability;
        private readonly ICSharpFunctionDeclaration _selectedFunctionDeclaration;

        public ComboEnsuresExecutor(ICSharpContextActionDataProvider provider, 
            AddContractClassAvailability addContractAvailability,
            ICSharpFunctionDeclaration selectedFunctionDeclaration)
            : base(provider)
        {
            Contract.Requires(provider != null);
            Contract.Requires(addContractAvailability != null);
            Contract.Requires(addContractAvailability.IsAvailable);

            Contract.Requires(selectedFunctionDeclaration != null);

            _provider = provider;
            _addContractAvailability = addContractAvailability;
            _selectedFunctionDeclaration = selectedFunctionDeclaration;
        }

        protected override void DoExecuteTransaction()
        {
            var addContractExecutor = new AddContractClassExecutor(_provider, _addContractAvailability, 
                _selectedFunctionDeclaration);
            addContractExecutor.Execute();

            var addEnsuresExecutor = EnsuresExecutor.CreateNotNullEnsuresExecutor(_provider, _selectedFunctionDeclaration);
            addEnsuresExecutor.ExecuteTransaction();
        }
    }
}