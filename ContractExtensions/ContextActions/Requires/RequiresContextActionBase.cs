using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon.Src.Bulbs.Resources;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;
using ReSharper.ContractExtensions.Settings;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContractClass(typeof (RequiresContextActionBaseContract))]
    public abstract class RequiresContextActionBase : ContextActionBase
    {
        protected bool _requiresShouldBeGeneric;
        protected ICSharpContextActionDataProvider _provider;

        protected RequiresContextActionBase(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_provider != null);
        }

        protected virtual IEnumerable<RequiresContextActionBase> GetContextActions()
        {
            if (IsGenericByDefault())
            {
                _requiresShouldBeGeneric = true;
            }
            yield return this;

            if (IsTestShell)
                yield break;

            var secondAction = CreateContextAction();
            secondAction._requiresShouldBeGeneric = !_requiresShouldBeGeneric;
            secondAction.IsAvailable(new UserDataHolder());
            yield return secondAction;
        }

        protected abstract string GetTextBase();

        protected abstract RequiresContextActionBase CreateContextAction();

        public override string Text
        {
            get
            {
                var result = GetTextBase();

                if (_requiresShouldBeGeneric)
                    result += " (with ArgumentNullException)";

                return result;
            }
        }

        protected virtual bool IsSingleItem()
        {
            return false;
        }

        protected bool IsTestShell { get { return Shell.Instance.IsTestShell; } }

        protected IList<IntentionAction> CreateBulbItemsBase()
        {
            return base.CreateBulbItems().ToList();
        } 

        public override IEnumerable<IntentionAction> CreateBulbItems()
        {
            if (IsSingleItem())
            {
                return base.CreateBulbItems();
            }

            var contextActions = GetContextActions().ToList();
            Contract.Assert(contextActions.Count > 0);

            var actions = contextActions[0].CreateBulbItemsBase();
            var anchor = actions[0].Anchor;

            var subMenuAnchor = new ExecutableGroupAnchor(
                anchor,
                IntentionsAnchors.ContextActionsAnchorPosition);

            return contextActions.Select(
                n => new IntentionAction(n, n.Text, BulbThemedIcons.ContextAction.Id, subMenuAnchor));


            //var contextActionsEnumerator = contextActions.GetEnumerator();
            //bool movedNext = contextActionsEnumerator.MoveNext();
            //Contract.Assert(movedNext, "ContextActionsEnumerator should have first element");

            //// Tests are not supports more than one menu item!
            //if (IsTestShell)
            //{
            //    return contextActionsEnumerator.Current.CreateBulbItemsBase();
            //}

            //var firstAction = contextActionsEnumerator.Current;
            //movedNext = contextActionsEnumerator.MoveNext();
            //Contract.Assert(movedNext, "ContextActionsEnumerator should have second element");
            //var secondAction = contextActionsEnumerator.Current;

            // Now we should create anchor and add subelements
            // based on configuration!
            //var actions = firstAction.CreateBulbItemsBase();

            // The anchor is different based on configuration settings


            //var subMenuAnchor = new ExecutableGroupAnchor(
            //    anchor,
            //    IntentionsAnchors.ContextActionsAnchorPosition);

            //return new List<IntentionAction>
            //{
            //    new IntentionAction(this, Text, BulbThemedIcons.ContextAction.Id, subMenuAnchor),
            //    new IntentionAction(secondAction, secondAction.Text,
            //        BulbThemedIcons.ContextAction.Id, subMenuAnchor)
            //};

            //bool genericByDefault = IsGenericByDefault();
            //if (genericByDefault)
            //{
            //    _requiresShouldBeGeneric = true;
            //}

            //var actions = base.CreateBulbItems().ToList();

            //// Tests are not supports more than one menu item!
            //if (IsTestShell)
            //{
            //    return actions;
            //}

            //// Now we should create anchor and add subelements
            //// based on configuration!
            //var secondAction = CreateContextAction();
            //secondAction._requiresShouldBeGeneric = !_requiresShouldBeGeneric;
            //secondAction.IsAvailable(new UserDataHolder());

            //// The anchor is different based on configuration settings
            //var anchor = actions[0].Anchor;

            //var subMenuAnchor = new ExecutableGroupAnchor(
            //    anchor,
            //    IntentionsAnchors.ContextActionsAnchorPosition);

            //return new List<IntentionAction>
            //{
            //    new IntentionAction(this, Text, BulbThemedIcons.ContextAction.Id, subMenuAnchor),
            //    new IntentionAction(secondAction, secondAction.Text,
            //        BulbThemedIcons.ContextAction.Id, subMenuAnchor)
            //};
        }

        protected IEnumerable<IntentionAction> CombineContextActions(params RequiresContextActionBase[] actions)
        {
            Contract.Requires(actions != null);
            Contract.Requires(actions.Length > 0);

            // Tests are not supports more than one menu item!
            if (Shell.Instance.IsTestShell)
            {
                return new IntentionAction[] {actions[0].CreateAction()};
            }


            throw new NotImplementedException();
        }

        private IntentionAction CreateAction(IAnchor anchor = null)
        {
            return new IntentionAction(this, Text, BulbThemedIcons.ContextAction.Id, anchor);
        }


        protected bool IsGenericByDefault()
        {
            var settings = _provider.SourceFile.GetSettingsStore()
                .GetKey<ContractExtensionsSettings>(SettingsOptimization.OptimizeDefault);

            return settings.UseGenericContractRequires;
        }
    }

    [ContractClassFor(typeof (RequiresContextActionBase))]
    abstract class RequiresContextActionBaseContract : RequiresContextActionBase
    {
        protected RequiresContextActionBaseContract(ICSharpContextActionDataProvider provider) : base(provider)
        {}

        protected override string GetTextBase()
        {
            Contract.Ensures(Contract.Result<string>() != null);
            throw new System.NotImplementedException();
        }

        protected override RequiresContextActionBase CreateContextAction()
        {
            Contract.Ensures(Contract.Result<RequiresContextActionBase>() != null);
            throw new System.NotImplementedException();
        }
    }
}