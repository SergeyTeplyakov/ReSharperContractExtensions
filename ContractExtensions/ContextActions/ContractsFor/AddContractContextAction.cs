using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class AddContractContextAction : ContractsContextActionBase
    {
        private const string MenuTextFormat = "Add or Update Contract Class for '{0}'";
        private const string Name = "Add Contract Class";
        private const string Description = "Add Contract Class for selected interface or abstract class.";

        private AddContractAvailability _addContractForAvailability = AddContractAvailability.Unavailable;

        public AddContractContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        protected override void ExecuteTransaction()
        {
            var executor = new AddContractExecutor(_provider, _addContractForAvailability);
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
            _addContractForAvailability = AddContractAvailability.IsAvailableForSelectedType(_provider);
            return _addContractForAvailability.IsAvailable;
        }
    }
}