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
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.UI.BulbMenu;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Settings;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Context action that adds Contract.Requiers with null check for all arguments.
    /// </summary>
    /// <remarks>
    /// Context action will add Contract.Requires only for argument available for this check 
    /// (i.e. all reference or nullable value types).
    /// </remarks>
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class ComboMethodRequiresContextAction : RequiresContextActionBase
    {
        private const string Name = "Method Contract.Requires";
        private const string Description = "Add Contract.Requires for all method arguments.";

        private const string Format = "Add not null Requires for all method arguments";

        private ComboMethodRequiresAvailability _availability = ComboMethodRequiresAvailability.Unavailable;

        public ComboMethodRequiresContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_availability != null);
        }

        protected override string GetTextBase()
        {
            return Format;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
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

            return null;
        }

        protected override RequiresContextActionBase CreateContextAction()
        {
            return new ComboMethodRequiresContextAction(_provider);
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _availability = ComboMethodRequiresAvailability.Create(_provider);
            return _availability.IsAvailable;
        }


        private void AddRequiresTo(ICSharpFunctionDeclaration contractFunction)
        {
            var executors = _availability.ArgumentNames
                .Select(pn => ArgumentCheckExecutor(pn, contractFunction));

            foreach (var executor in executors)
            {
                executor.ExecuteTransaction();
            }
        }

        private ArgumentRequiresExecutor ArgumentCheckExecutor(string argumentName,
            ICSharpFunctionDeclaration functionWithContract)
        {
            return new ArgumentRequiresExecutor(_provider, _requiresShouldBeGeneric, functionWithContract, argumentName);
        }


        private ICSharpFunctionDeclaration GetContractFunction()
        {
            return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true)
                .Return(x => x.GetContractFunction());
        }

        private void AddContractClass()
        {
            Contract.Assert(_availability.AddContractAvailability != null,
                "Adding contract class requires AddContractAvailability!");

            var addContractExecutor = new AddContractExecutor(
                _provider, _availability.AddContractAvailability,
                _availability.SelectedFunctionDeclaration);

            addContractExecutor.Execute();
        }
    }
}