using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class AddRequiresContextAction : RequiresContextActionBase
    {
        private AddRequiresAvailability _argumentRequiresAvailability = AddRequiresAvailability.Unavailable;

        private const string Name = "Add Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Requires '{0}' is not null";

        public AddRequiresContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_argumentRequiresAvailability != null);
        }

        protected override void ExecuteTransaction()
        {
            Contract.Assert(_argumentRequiresAvailability.IsAvailable);

            var executor = new AddRequiresExecutor(_provider, _requiresShouldBeGeneric,
                _argumentRequiresAvailability.FunctionToInsertPrecondition,
                _argumentRequiresAvailability.SelectedParameterName,
                _argumentRequiresAvailability.SelectedParameterType);

            executor.ExecuteTransaction();
        }

        protected override bool DoIsAvailable()
        {
            _argumentRequiresAvailability = AddRequiresAvailability.CheckIsAvailable(_provider);
            return _argumentRequiresAvailability.IsAvailable;
        }

        protected override string GetTextBase()
        {
            return string.Format(Format, _argumentRequiresAvailability.SelectedParameterName);
        }

        protected override RequiresContextActionBase CreateContextAction()
        {
            return new AddRequiresContextAction(_provider);
        }
    }
}