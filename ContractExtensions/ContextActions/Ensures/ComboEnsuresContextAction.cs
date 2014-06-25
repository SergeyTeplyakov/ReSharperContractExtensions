using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class ComboEnsuresContextAction : ContextActionBase
    {
        private const string MenuText = "Add Ensures result is not null in contract class";
        private const string Name = "Combo Add Contract.Ensures";
        private const string Description = "Add Contract Class and ensures that result is not null.";

        private readonly ICSharpContextActionDataProvider _provider;
        private ComboEnsuresAvailability _comboEnsuresAvailability = ComboEnsuresAvailability.Unavailable;

        public ComboEnsuresContextAction(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_comboEnsuresAvailability != null);
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_comboEnsuresAvailability.IsAvailable);

            var executor = new ComboEnsuresExecutor(_provider, _comboEnsuresAvailability.AddContractAvailability,
                _comboEnsuresAvailability.SelectedFunction);
            executor.ExecuteTransaction();

            return null;
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _comboEnsuresAvailability = new ComboEnsuresAvailability(_provider);
            return _comboEnsuresAvailability.IsAvailable;
        }

        public override string Text { get { return MenuText; } }
    }
}