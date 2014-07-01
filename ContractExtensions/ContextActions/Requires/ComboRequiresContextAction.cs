using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.ContractUtils;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class ComboRequiresContextAction : RequiresContextActionBase
    {
        private const string Name = "Combo Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Add requires '{0}' is not null in contract class";

        private ComboRequiresAvailability _availability = ComboRequiresAvailability.Unavailable;

        public ComboRequiresContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        protected override void ExecuteTransaction()
        {
            Contract.Assert(_availability.IsAvailable);

            var contractFunction = GetContractFunction();
            if (contractFunction == null)
            {
                AddContractClass();

                contractFunction = GetContractFunction();
                Contract.Assert(contractFunction != null);
            }

            AddRequiresTo(contractFunction);
        }

        [CanBeNull, System.Diagnostics.Contracts.Pure]
        private ICSharpFunctionDeclaration GetContractFunction()
        {
            return _availability.SelectedFunction.GetContractFunction();
        }

        protected override string GetTextBase()
        {
            return string.Format(Format, _availability.ParameterName);
        }

        protected override RequiresContextActionBase CreateContextAction()
        {
            return new ComboRequiresContextAction(_provider);
        }

        protected override bool DoIsAvailable()
        {
            _availability = ComboRequiresAvailability.CheckIsAvailable(_provider);
            return _availability.IsAvailable;
        }

        private void AddContractClass()
        {
            var addContractExecutor = new AddContractExecutor(
                _provider, _availability.AddContractAvailability,
                _availability.SelectedFunction);

            addContractExecutor.Execute();
        }

        private void AddRequiresTo(ICSharpFunctionDeclaration contractFunction)
        {
            var addRequiresExecutor = new ArgumentRequiresExecutor(_provider, _requiresShouldBeGeneric,
                contractFunction, _availability.ParameterName, _availability.ParameterType);
            addRequiresExecutor.ExecuteTransaction();
        }


    }
}