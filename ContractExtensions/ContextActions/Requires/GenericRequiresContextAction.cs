using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Daemon.Src.Bulbs.Resources;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.UI.BulbMenu;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class GenericRequiresContextAction : RequiresContextActionBase
    {
        private const string Name = "Add Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Requires '{0}' is not null";

        public GenericRequiresContextAction(ICSharpContextActionDataProvider provider) 
            : base(provider, true)
        { }

        protected override string GetText(string selectedParameterName)
        {
            return string.Format(Format, selectedParameterName);
        }

        // I've added trick for creating submenu to generic context action,
        // because I'm using tests for non-generic version, and right now
        // test infrastructure does not support actions with multiple bulb items.
        public override IEnumerable<IntentionAction> CreateBulbItems()
        {
            var nonGeneric = new RequiresContextAction(_provider);
            nonGeneric.IsAvailable(_cache);

            var actions = nonGeneric.CreateBulbItems().ToList();

            // TODO: add configuration to check, what action should be first: generic or not!
            var subMenuAnchor = new ExecutableGroupAnchor(
                actions[0].Anchor,
                IntentionsAnchors.ContextActionsAnchorPosition);

            return new List<IntentionAction>
            {
                new IntentionAction(nonGeneric, nonGeneric.Text,
                    BulbThemedIcons.ContextAction.Id, subMenuAnchor),
                new IntentionAction(this, Text, BulbThemedIcons.ContextAction.Id, subMenuAnchor),
            };
        }

    }
}