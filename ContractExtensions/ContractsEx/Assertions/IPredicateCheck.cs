using System.Diagnostics.Contracts;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    [ContractClass(typeof (IPredicateCheckContract))]
    public interface IPredicateCheck
    {
        string ArgumentName { get; }
        bool ChecksForNotNull(string name);
        bool ChecksForNull(string name);
    }

    [ContractClassFor(typeof (IPredicateCheck))]
    abstract class IPredicateCheckContract : IPredicateCheck
    {
        public virtual string ArgumentName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new System.NotImplementedException();
            }
        }

        public virtual bool ChecksForNotNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            throw new System.NotImplementedException();
        }

        bool IPredicateCheck.ChecksForNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            throw new System.NotImplementedException();
        }
    }
}