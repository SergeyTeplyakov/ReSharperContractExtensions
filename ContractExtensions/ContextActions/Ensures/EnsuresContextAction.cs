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
using ReSharper.ContractExtensions.ContextActions.Ensures;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;

namespace ReSharper.ContractExtensions.ContextActions
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 90)]
    public class EnsuresContextAction : ContractsContextActionBase
    {
        private const string MenuText = "Ensures result is not null";
        private const string Name = "Add Contract.Ensures";
        private const string Description = "Add Contract.Ensures on the potentially nullable return value.";

        private EnsuresAvailability _ensuresAvailability = EnsuresAvailability.Unavailable;

        public EnsuresContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_ensuresAvailability != null);
        }

        protected override void ExecuteTransaction()
        {
            Contract.Assert(_ensuresAvailability.IsAvailable);

            var executor = EnsuresExecutor.CreateNotNullEnsuresExecutor(_provider, _ensuresAvailability.SelectedFunction);
            executor.ExecuteTransaction();
        }

        public override string Text
        {
            get { return MenuText; }
        }

        protected override bool DoIsAvailable()
        {
            _ensuresAvailability = EnsuresAvailability.IsAvailableForNullableResult(_provider);
            return _ensuresAvailability.IsAvailable;
        }
    }
}