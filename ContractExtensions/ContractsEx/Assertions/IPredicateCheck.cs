using System.Diagnostics.Contracts;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    [ContractClass(typeof (IPredicateCheckContract))]
    public interface IPredicateCheck
    {
        string ArgumentName { get; }

        [Pure]
        bool ChecksForNull(string name);
    }

    [ContractClassFor(typeof (IPredicateCheck))]
    abstract class IPredicateCheckContract : IPredicateCheck
    {
        string IPredicateCheck.ArgumentName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new System.NotImplementedException();
            }
        }

        bool IPredicateCheck.ChecksForNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            throw new System.NotImplementedException();
        }
    }
}