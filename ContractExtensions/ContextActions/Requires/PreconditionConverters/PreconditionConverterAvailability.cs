using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    internal sealed class PreconditionConverterAvailability : ContextActionAvailabilityBase<PreconditionConverterAvailability>
    {
        private ContractPreconditionAssertion _contractPreconditionAssertion;

        public PreconditionConverterAvailability()
        {}

        public PreconditionConverterAvailability(ICSharpContextActionDataProvider provider) : base(provider)
        {}

        protected override void CheckAvailability()
        {
            _contractPreconditionAssertion = GetSelectedPreconditionAssertion();
            _isAvailable = _contractPreconditionAssertion != null;
        }

        public ContractPreconditionAssertion PreconditionAssertion
        {
            get
            {
                Contract.Ensures(!IsAvailable || Contract.Result<ContractPreconditionAssertion>() != null);
                return _contractPreconditionAssertion;
            }
        }

        public PreconditionType SourcePreconditionType
        {
            get
            {
                Contract.Requires(IsAvailable);
                return PreconditionAssertion.PreconditionType;
            }
        }

        [CanBeNull]
        private ContractPreconditionAssertion GetSelectedPreconditionAssertion()
        {
            var statement = _provider.GetSelectedElement<ICSharpStatement>(true, true);
            if (statement == null)
            {
                statement =
                    _provider.GetSelectedElement<IExpressionStatement>(true, true)
                        .Return(x => x.GetContainingStatement());
            }

            return statement.Return(ContractPreconditionAssertion.TryCreate);
        }
    }
}