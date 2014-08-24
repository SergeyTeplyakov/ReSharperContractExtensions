using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class AddContractClassContextAction : ContractsContextActionBase
    {
        private const string MenuTextFormat = "Add or Update Contract Class for '{0}'";
        private const string Name = "Add Contract Class";
        private const string Description = "Add Contract Class for selected interface or abstract class.";

        private AddContractClassAvailability _addContractForAvailability = AddContractClassAvailability.Unavailable;

        public AddContractClassContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        protected override void ExecuteTransaction()
        {
            var executor = new AddContractClassExecutor(_provider, _addContractForAvailability);
            executor.Execute();
        }

        public override string Text
        {
            get
            {
                Contract.Assert(_addContractForAvailability.IsAvailable);
                return string.Format(MenuTextFormat, _addContractForAvailability.SelectedDeclarationTypeName);
            }
        }

        protected override bool DoIsAvailable()
        {
            _addContractForAvailability = AddContractClassAvailability.IsAvailableForSelectedType(_provider);
            return _addContractForAvailability.IsAvailable;
        }
    }
}