using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.Src.Bulbs.Resources;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.ContractUtils;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class ComboRequiresContextAction : ContextActionBase
    {
        private bool _isRequiesStatementGeneric;
        private readonly ICSharpContextActionDataProvider _provider;
        private IUserDataHolder _cache;

        private const string Name = "Combo Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Add requires '{0}' is not null in contract class";

        private ComboRequiresAvailability _availability = ComboRequiresAvailability.Unavailable;

        public ComboRequiresContextAction(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_availability.IsAvailable);

            var contractFunction = GetContractFunction();
            if (contractFunction == null)
            {
                AddContractClass();

                contractFunction = GetContractFunction();
                Contract.Assert(contractFunction != null);
            }

            AddRequiresTo(contractFunction);

            return null;
        }

        [CanBeNull, System.Diagnostics.Contracts.Pure]
        private ICSharpFunctionDeclaration GetContractFunction()
        {
            return _availability.SelectedFunction.GetContractFunction();
        }


        public override string Text
        {
            get
            {
                Contract.Assert(_availability.IsAvailable);
                var result = string.Format(Format, _availability.ParameterName);

                

                return result;
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _cache = cache;
            _availability = new ComboRequiresAvailability(_provider);
            return _availability.IsAvailable;
        }


        private void AddContractClass()
        {
            var addContractExecutor = new AddContractExecutor(
                _provider, _availability.AddContractAvailability,
                _availability.SelectedFunction);

            addContractExecutor.Execute();
        }

        private void AddRequiresTo(ICSharpFunctionDeclaration contractFunction)
        {
            var addRequiresExecutor = new RequiresExecutor(_provider, _isRequiesStatementGeneric,
                contractFunction, _availability.ParameterName);
            addRequiresExecutor.ExecuteTransaction();
        }

        public override IEnumerable<IntentionAction> CreateBulbItems()
        {
            var actions = base.CreateBulbItems().ToList();
            if (Shell.Instance.IsTestShell)
            {
                return actions;
            }

            var generic = new ComboRequiresContextAction(_provider) {_isRequiesStatementGeneric = true};
            generic.IsAvailable(_cache);

            // TODO: add configuration to check, what action should be first: generic or not!
            var subMenuAnchor = new ExecutableGroupAnchor(
                actions[0].Anchor,
                IntentionsAnchors.ContextActionsAnchorPosition);

            return new List<IntentionAction>
            {
                new IntentionAction(this, Text, BulbThemedIcons.ContextAction.Id, subMenuAnchor),
                new IntentionAction(generic, generic.Text,
                    BulbThemedIcons.ContextAction.Id, subMenuAnchor),
            };
        }
    }
}