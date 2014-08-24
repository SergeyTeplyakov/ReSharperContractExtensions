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
        private IPrecondition _contractRequires;

        public PreconditionConverterAvailability()
        {}

        public PreconditionConverterAvailability(ICSharpContextActionDataProvider provider) : base(provider)
        {}

        protected override void CheckAvailability()
        {
            _contractRequires = GetSelectedRequires();
            _isAvailable = _contractRequires != null;
        }

        public IPrecondition Requires
        {
            get
            {
                Contract.Ensures(!IsAvailable || Contract.Result<IPrecondition>() != null);
                return _contractRequires;
            }
        }

        public PreconditionType SourcePreconditionType
        {
            get
            {
                Contract.Requires(IsAvailable);
                return Requires.PreconditionType;
            }
        }

        [CanBeNull]
        private IPrecondition GetSelectedRequires()
        {
            var statement = _provider.GetSelectedElement<ICSharpStatement>(true, true);
            if (statement == null)
            {
                statement =
                    _provider.GetSelectedElement<IExpressionStatement>(true, true)
                        .Return(x => x.GetContainingStatement());
            }

            return statement.Return(ContractStatementFactory.TryCreatePrecondition);
        }
    }
}