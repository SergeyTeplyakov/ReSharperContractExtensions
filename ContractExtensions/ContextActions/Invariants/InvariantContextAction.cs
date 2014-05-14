using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ContextActions.Invariants
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 90)]
    public class InvariantContextAction : ContextActionBase
    {
        private const string MenuFormat = "Invariant '{0}' is not null";
        private const string Name = "Add Contract.Invariant";
        private const string Description = "Add Contract.Invariant on the selected field or property.";
        private readonly ICSharpContextActionDataProvider _provider;

        private InvariantAvailability _invariantContract = InvariantAvailability.InvariantUnavailable;

        public InvariantContextAction(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_provider != null);
            Contract.Invariant(_invariantContract != null);
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_invariantContract.IsAvailable);

            var executor = new InvariantActionExecutor(_invariantContract, _provider);
            executor.ExecuteTransaction(solution, progress);

            return null;
        }

        public override string Text
        {
            get { return string.Format(MenuFormat, _invariantContract.SelectedMemberName); }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _invariantContract = InvariantAvailability.Create(_provider);
            return _invariantContract.IsAvailable;
        }
    }
}