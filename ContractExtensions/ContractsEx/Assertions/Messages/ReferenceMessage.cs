using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public sealed class ReferenceMessage : Message
    {
        private readonly IReferenceExpression _reference;

        public ReferenceMessage(IExpression originalExpression, IReferenceExpression reference)
            : base(originalExpression)
        {
            Contract.Requires(reference != null);
            _reference = reference;
        }

        public IReferenceExpression Reference
        {
            get
            {
                Contract.Ensures(Contract.Result<IReferenceExpression>() != null);
                return _reference;
            }
        }
    }
}