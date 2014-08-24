using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using ReSharper.ContractExtensions.ContractsEx.Assertions;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    public enum PreconditionConverterType
    {
        FromIfThrow,
        FromContractRequires,
        FromGenericContractRequires,
        FromGuardClause,
    }

    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class ContractRequiresToGenericContractRequiresContextAction : PreconditionConverterContextAction
    {
        private const string Name = "Requires to Requires<ArgumentNullException>";
        private const string Description = "Convert Contract.Requires to Contract.Requires<ArgumentNullException>.";

        public ContractRequiresToGenericContractRequiresContextAction(ICSharpContextActionDataProvider provider) 
            : base(provider)
        {}

        public override string Text
        {
            get
            {
                var requires = (ContractRequires)_availability.Requires;
                string exceptionType = requires.PotentialGenericVersionException().ShortName;
                return string.Format("Convert to Contract.Requires<{0}>", exceptionType);
            }
        }

        internal override void Execute()
        {
            var executor = new PreconditionConverterExecutor(_availability, PreconditionType.GenericContractRequires);
            executor.ExecuteTransaction();
        }

        internal override bool IsAvailable(PreconditionConverterAvailability availability)
        {
            return availability.IsAvailable && availability.SourcePreconditionType == PreconditionType.ContractRequires;
        }

        protected override bool IsSingleItem()
        {
            return true;
        }
    }

    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class GenericContractRequiresToContractRequiresContextAction : PreconditionConverterContextAction
    {
        private const string Name = "Requires<ArgumentNullException> to Requires";
        private const string Description = "Convert Contract.Requires to Contract.Requires<ArgumentNullException>.";

        public GenericContractRequiresToContractRequiresContextAction(ICSharpContextActionDataProvider provider) 
            : base(provider)
        {}

        public override string Text
        {
            get { return "Convert to non-generic Contract.Requires"; }
        }

        internal override void Execute()
        {
            var executor = new PreconditionConverterExecutor(_availability, PreconditionType.ContractRequires);
            executor.ExecuteTransaction();
        }

        internal override bool IsAvailable(PreconditionConverterAvailability availability)
        {
            return availability.IsAvailable && availability.SourcePreconditionType == PreconditionType.GenericContractRequires;
        }

        protected override bool IsSingleItem()
        {
            return true;
        }
    }

    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class IfThrowToContractRequiresContextAction : PreconditionConverterContextAction
    {
        private const string Name = "If-Throw to Contract.Requires";
        private const string Description = "Convert if-throw precondition to Contract.Requires";

        public IfThrowToContractRequiresContextAction(ICSharpContextActionDataProvider provider) 
            : base(provider)
        {}

        public override string Text
        {
            get
            {
                if (!_requiresShouldBeGeneric)
                    return "Convert to Contract.Requires";

                var requires = (IfThrowPrecondition) _availability.Requires;
                string exceptionType = requires.ExceptionTypeName.ShortName;
                return string.Format("Convert to Contract.Requires<{0}>", exceptionType);
            }
        }

        internal override void Execute()
        {
            var destination = _requiresShouldBeGeneric
                ? PreconditionType.GenericContractRequires
                : PreconditionType.ContractRequires;

            var executor = new PreconditionConverterExecutor(_availability, destination);
            executor.ExecuteTransaction();
        }

        internal override bool IsAvailable(PreconditionConverterAvailability availability)
        {
            return availability.IsAvailable && availability.SourcePreconditionType == PreconditionType.IfThrowStatement;
        }

        protected override RequiresContextActionBase CreateContextAction()
        {
            return new IfThrowToContractRequiresContextAction(_provider);
        }
    }

    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class PreconditionConverterContextAction : RequiresContextActionBase
    {
        private const string Name = "Convert Contract.Requires";
        private const string Description = "Convert any contract check to Contract.Requires.";

        internal PreconditionConverterAvailability _availability = PreconditionConverterAvailability.Unavailable;

        public PreconditionConverterContextAction(ICSharpContextActionDataProvider provider) 
            : base(provider)
        {}

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_availability != null);
        }

        protected override sealed void ExecuteTransaction()
        {
            Execute();
        }

        internal virtual void Execute()
        {
            
        }

        internal virtual bool IsAvailable(PreconditionConverterAvailability availability)
        {
            // Dirty hack! Instance of PreconditionConverterContextAction should be used
            // and visible only for testing!!
            if (GetType() == typeof (PreconditionConverterContextAction) && !IsTestShell)
                return false;

            return availability.IsAvailable;
        }

        protected override bool DoIsAvailable()
        {
            _availability = PreconditionConverterAvailability.CheckIsAvailable(_provider);
            return IsAvailable(_availability);
        }

        protected override string GetTextBase()
        {
            Contract.Assert(_availability.IsAvailable);
            return "foo";
        }

        protected override RequiresContextActionBase CreateContextAction()
        {
            return new PreconditionConverterContextAction(_provider);
        }
    }
}