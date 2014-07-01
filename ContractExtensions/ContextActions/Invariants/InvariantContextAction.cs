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

namespace ReSharper.ContractExtensions.ContextActions.Invariants
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 90)]
    public class InvariantContextAction : ContractsContextActionBase
    {
        private const string MenuFormat = "Invariant '{0}' is not null";
        private const string Name = "Add Contract.Invariant";
        private const string Description = "Add Contract.Invariant on the selected field or property.";

        private InvariantAvailability _invariantContract = InvariantAvailability.InvariantUnavailable;

        public InvariantContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_invariantContract != null);
        }

        protected override void ExecuteTransaction()
        {
            Contract.Assert(_invariantContract.IsAvailable);

            var executor = new InvariantActionExecutor(_invariantContract, _provider);
            executor.ExecuteTransaction();
        }

        public override string Text
        {
            get { return string.Format(MenuFormat, _invariantContract.SelectedMemberName); }
        }

        protected override bool DoIsAvailable()
        {
            _invariantContract = InvariantAvailability.Create(_provider);
            return _invariantContract.IsAvailable;
        }
    }
}