using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;

using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Infrastructure
{
    public abstract class ContractsContextActionBase : ContextActionBase
    {
        protected readonly ICSharpContextActionDataProvider _provider;

        protected ContractsContextActionBase(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            _provider = provider;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            ExecuteTransaction();

            return null;
        }

        protected abstract void ExecuteTransaction();

        public override sealed bool IsAvailable(IUserDataHolder cache)
        {
            if (!_provider.IsValidForContractContextActions())
                return false;

            return DoIsAvailable();
        }

        protected abstract bool DoIsAvailable();
    }
}