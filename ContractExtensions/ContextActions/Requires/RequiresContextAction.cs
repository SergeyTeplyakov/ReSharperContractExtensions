using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.ReSharper.Daemon.Src.Bulbs.Resources;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.UI.BulbMenu;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class RequiresContextAction : RequiresContextActionBase
    {
        private const string Name = "Add Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Requires '{0}' is not null";

        public RequiresContextAction(ICSharpContextActionDataProvider provider) 
            : base(provider, false)
        {}

        protected override string GetText(string selectedParameterName)
        {
            return string.Format(Format, selectedParameterName);
        }

        public override IEnumerable<IntentionAction> CreateBulbItems()
        {
            var actions = base.CreateBulbItems().ToList();

            if (Shell.Instance.IsTestShell)
            {
                return actions;
            }

            var generic = new GenericRequiresContextAction(_provider);
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