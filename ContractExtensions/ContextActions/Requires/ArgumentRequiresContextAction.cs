using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.TextControl;
using JetBrains.Util;

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
            Contract.Assert(_argumentRequiresAvailability._isAvailable);

            var executor = new ArgumentRequiresExecutor(_provider, _requiresShouldBeGeneric,
                _argumentRequiresAvailability.FunctionToInsertPrecondition,
                _argumentRequiresAvailability.SelectedParameterName,
                _argumentRequiresAvailability.SelectedParameterType);

            executor.ExecuteTransaction();

            return null;
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _argumentRequiresAvailability = ArgumentRequiresAvailability.CheckIsAvailable(_provider);
            return _argumentRequiresAvailability._isAvailable;
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