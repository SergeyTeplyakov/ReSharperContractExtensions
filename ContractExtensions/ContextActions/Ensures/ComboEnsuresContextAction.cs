using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class ComboEnsuresContextAction : ContractsContextActionBase
    {
        private const string MenuText = "Add Ensures result is not null in contract class";
        private const string Name = "Combo Add Contract.Ensures";
        private const string Description = "Add Contract Class and ensures that result is not null.";

        private ComboEnsuresAvailability _comboEnsuresAvailability = ComboEnsuresAvailability.Unavailable;

        public ComboEnsuresContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_comboEnsuresAvailability != null);
        }

        protected override void ExecuteTransaction()
        {
            Contract.Assert(_comboEnsuresAvailability.IsAvailable);

            var executor = new ComboEnsuresExecutor(_provider, _comboEnsuresAvailability.AddContractAvailability,
                _comboEnsuresAvailability.SelectedFunction);
            executor.ExecuteTransaction();
        }

        protected override bool DoIsAvailable()
        {
            _comboEnsuresAvailability = new ComboEnsuresAvailability(_provider);
            return _comboEnsuresAvailability.IsAvailable;
        }

        public override string Text { get { return MenuText; } }
    }
}