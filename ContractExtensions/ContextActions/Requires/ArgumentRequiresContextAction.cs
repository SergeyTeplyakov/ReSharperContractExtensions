using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.Src.Bulbs.Resources;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;
using ReSharper.ContractExtensions.Settings;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class ArgumentRequiresContextAction : RequiresContextActionBase
    {
        private ArgumentRequiresAvailability _argumentRequiresAvailability = ArgumentRequiresAvailability.Unavailable;

        private const string Name = "Add Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Requires '{0}' is not null";

        public ArgumentRequiresContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_argumentRequiresAvailability != null);
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_argumentRequiresAvailability.IsAvailable);

            var executor = new ArgumentRequiresExecutor(_provider, _requiresShouldBeGeneric,
                _argumentRequiresAvailability.FunctionToInsertPrecondition,
                _argumentRequiresAvailability.SelectedParameterName,
                _argumentRequiresAvailability.SelectedParameterType);

            executor.ExecuteTransaction();

            return null;
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _argumentRequiresAvailability = ArgumentRequiresAvailability.Create(_provider);
            return _argumentRequiresAvailability.IsAvailable;
        }

        protected override string GetTextBase()
        {
            return string.Format(Format, _argumentRequiresAvailability.SelectedParameterName);
        }

        protected override RequiresContextActionBase CreateContextAction()
        {
            return new ArgumentRequiresContextAction(_provider);
        }
    }
}