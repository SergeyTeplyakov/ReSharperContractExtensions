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

namespace ReSharper.ContractExtensions.ContextActions
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 90)]
    public class EnsuresContextAction : ContextActionBase
    {
        private const string MenuText = "Ensures result is not null";
        private const string Name = "Add Contract.Ensures";
        private const string Description = "Add Contract.Ensures on the potentially nullable return value.";

        private readonly ICSharpContextActionDataProvider _provider;

        private EnsuresAvailability _ensuresAvailability = EnsuresAvailability.Unavailable;

        public EnsuresContextAction(ICSharpContextActionDataProvider provider)
        {
            _provider = provider;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_ensuresAvailability != null);
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_ensuresAvailability.IsAvailable);

            var executor = new EnsuresExecutor(_provider, _ensuresAvailability.SelectedFunction);
            executor.Execute(solution, progress);

            return null;
        }

        public override string Text
        {
            get { return MenuText; }
        }

        [Pure]
        public override bool IsAvailable(IUserDataHolder cache)
        {
            _ensuresAvailability = new EnsuresAvailability(_provider);
            return _ensuresAvailability.IsAvailable;
        }
    }
}