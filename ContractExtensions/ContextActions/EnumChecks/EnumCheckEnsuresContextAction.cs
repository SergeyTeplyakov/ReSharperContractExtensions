using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.Ensures;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;

namespace ReSharper.ContractExtensions.ContextActions.EnumChecks
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class EnumCheckEnsuresContextAction : ContractsContextActionBase
    {
        private const string MenuText = "Ensures result is valid by Enum.IsDefined";
        private const string Name = "Combo Add Contract.Ensures";
        private const string Description = "Add Contract Class and ensures that result is not null.";

        private EnsuresAvailability _availability = EnsuresAvailability.Unavailable;

        public EnumCheckEnsuresContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        protected override void ExecuteTransaction()
        {
            var executor = EnsuresExecutor.CreateEnumIsValidEnsuresExecutor(_provider, _availability.SelectedFunction);
            executor.ExecuteTransaction();
        }

        protected override bool DoIsAvailable()
        {
            _availability = EnsuresAvailability.IsAvailableForEnumResult(_provider);
            return _availability.IsAvailable;
        }

        public override string Text { get { return MenuText; } }
    }
}