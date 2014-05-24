using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class AddContractForContextAction : ContextActionBase
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private const string MenuTextFormat = "Add Contract Invariant for '{0}'";
        private const string Name = "Add Contract Invariant";
        private const string Description = "Add Contract Invariant for selected interface or abstract class.";

        private AddContractForAvailability _addContractForAvailability = AddContractForAvailability.Unavailable;

        public AddContractForContextAction(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            _provider = provider;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var executor = new AddContractForExecutor(_addContractForAvailability, _provider);
            executor.Execute(solution, progress);

            return null;
        }

        public override string Text
        {
            get
            {
                Contract.Assert(_addContractForAvailability.IsAvailable);
                return string.Format(MenuTextFormat, _addContractForAvailability.SelectedDeclaration);
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _addContractForAvailability = new AddContractForAvailability(_provider, false);
            return _addContractForAvailability.IsAvailable;
        }
    }
}