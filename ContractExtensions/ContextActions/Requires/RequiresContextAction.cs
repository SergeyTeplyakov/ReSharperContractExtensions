using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
    public sealed class RequiresContextAction : ContextActionBase
    {
        private const string Name = "Add Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";
        private const string Format = "Requires '{0}' is not null";

        private readonly ICSharpContextActionDataProvider _provider;
        private RequiresAvailability _requiresAvailability = RequiresAvailability.Unavailable;

        private IUserDataHolder _cache;

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

            var executor = new RequiresExecutor(_requiresAvailability, _provider, IsGeneric);
            executor.ExecuteTransaction(solution, progress);

            return null;
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _cache = cache;

            _requiresAvailability = RequiresAvailability.Create(_provider);

            return _requiresAvailability.IsAvailable;
        }

        public override string Text
        {
            get { return GetText(); }
        }

        public override IEnumerable<IntentionAction> CreateBulbItems()
        {
            var actions = base.CreateBulbItems().ToList();

            // TODO: add configuration to check, what action should be first: generic or not!
            var subMenuAnchor = new ExecutableGroupAnchor(
                actions[0].Anchor,
                IntentionsAnchors.ContextActionsAnchorPosition);

            var genericRequiresContextAction = new RequiresContextAction(_provider) {IsGeneric = true};
            genericRequiresContextAction.IsAvailable(_cache);

            return new List<IntentionAction>
            {
                new IntentionAction(this, Text, BulbThemedIcons.ContextAction.Id, subMenuAnchor),
                new IntentionAction(genericRequiresContextAction, genericRequiresContextAction.Text, BulbThemedIcons.ContextAction.Id, subMenuAnchor),
            };
        }

        public bool IsGeneric { get; set; }

        private string GetText()
        {
            // WTF: Text property could be accessed even if IsAvailable returns false???
            var result = string.Format(Format, _requiresAvailability.SelectedParameterName);
            if (IsGeneric)
                result += " (with ArgumentNullException)";

            return result;
        }
    }
}