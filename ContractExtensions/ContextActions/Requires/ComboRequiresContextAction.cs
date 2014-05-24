using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.ContractUtils;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class ComboRequiresContextAction : ContextActionBase
    {
        private readonly ICSharpContextActionDataProvider _provider;

        private const string Name = "Combo Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Add contract class and requires '{0}' is not null";

        private ComboRequiresAvailability _availability = ComboRequiresAvailability.Unavailable;

        public ComboRequiresContextAction(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_availability.IsAvailable);

            var addContractExecutor = new AddContractForExecutor(_availability.AddContractAvailability, _provider);
            addContractExecutor.Execute(solution, progress);

            var functionDeclaration = GetFunctionForContract();
            var addRequiresExecutor = new RequiresExecutor(_provider, false,
                    functionDeclaration, _availability.ParameterName);
            addRequiresExecutor.ExecuteTransaction(solution, progress);

            return null;
        }

        public override string Text
        {
            get
            {
                Contract.Assert(_availability.IsAvailable);
                return string.Format(Format, _availability.ParameterName);
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _availability = new ComboRequiresAvailability(_provider);
            return _availability.IsAvailable;
        }

        private ICSharpFunctionDeclaration GetFunctionForContract()
        {
            var functionDeclaration = _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
            return functionDeclaration.GetContractFunction();
        }
    }
}