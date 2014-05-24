using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.Src.Bulbs.Resources;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.TextControl;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class RequiresContextAction : ContextActionBase
    {
        protected ICSharpContextActionDataProvider _provider;
        protected IUserDataHolder _cache;
        private RequiresAvailability _requiresAvailability = RequiresAvailability.Unavailable;
        private bool _isGeneric;

        private const string Name = "Add Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Requires '{0}' is not null";

        public RequiresContextAction(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            _provider = provider;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_provider != null);
            Contract.Invariant(_requiresAvailability != null);
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_requiresAvailability.IsAvailable);

            var executor = new RequiresExecutor(_provider, _isGeneric,
                _requiresAvailability.FunctionToInsertPrecondition,
                _requiresAvailability.SelectedParameterName);

            executor.ExecuteTransaction(solution, progress);

            return null;
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _requiresAvailability = RequiresAvailability.Create(_provider);
            _cache = cache;

            return _requiresAvailability.IsAvailable;
        }

        public override string Text
        {
            get
            {
                Contract.Assert(_requiresAvailability.IsAvailable);
                string result = string.Format(Format, _requiresAvailability.SelectedParameterName);
                if (_isGeneric)
                    result += " (with ArgumentNullException)";
                return result;
            }
        }

        public override IEnumerable<IntentionAction> CreateBulbItems()
        {
            var actions = base.CreateBulbItems().ToList();

            if (Shell.Instance.IsTestShell)
            {
                return actions;
            }

            var generic = new RequiresContextAction(_provider) {_isGeneric = true};
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