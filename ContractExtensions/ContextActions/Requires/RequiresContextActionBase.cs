using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    public abstract class RequiresContextActionBase : ContextActionBase
    {
        protected readonly ICSharpContextActionDataProvider _provider;
        protected IUserDataHolder _cache;

        private RequiresAvailability _requiresAvailability = RequiresAvailability.Unavailable;
        private readonly bool _isGeneric;


        protected RequiresContextActionBase(ICSharpContextActionDataProvider provider, bool isGeneric)
        {
            Contract.Requires(provider != null);

            _provider = provider;
            _isGeneric = isGeneric;
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
                return GetText(_requiresAvailability.SelectedParameterName);
            }
        }

        protected abstract string GetText(string selectedParameterName);

    }
}