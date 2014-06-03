using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.ContractUtils;
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
    public class ComboMethodRequiresContextAction : ContextActionBase
    {
        private bool _isRequiesStatementGeneric;
        private readonly ICSharpContextActionDataProvider _provider;
        private IUserDataHolder _cache;

        private const string Name = "Method Contract.Requires";
        private const string Description = "Add Contract.Requires for all method arguments.";

        private const string Format = "Add not null Requires for all method arguments";

        private ComboMethodRequiresAvailability _availability = ComboMethodRequiresAvailability.Unavailable;

        public ComboMethodRequiresContextAction(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            _provider = provider;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_availability != null);
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

        private void AddRequiresTo(ICSharpFunctionDeclaration contractFunction)
        {
            var executors = _availability.ArgumentNames
                .Select(pn => ArgumentCheckExecutor(pn, contractFunction));

            foreach (var executor in executors)
            {
                executor.ExecuteTransaction();
            }
        }

        private RequiresExecutor ArgumentCheckExecutor(string argumentName,
            ICSharpFunctionDeclaration functionWithContract)
        {
            return new RequiresExecutor(_provider, _isRequiesStatementGeneric, functionWithContract, argumentName);
        }

        public override string Text
        {
            get
            {
                var result = Format;

                if (_isRequiesStatementGeneric)
                    result += " (with ArgumentNullException)";

                return result;
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            _availability = ComboMethodRequiresAvailability.Create(_provider);
            return _availability.IsAvailable;
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