using System.Diagnostics.Contracts;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public abstract class PredicateCheck
    {
        private readonly string _argumentName;

        protected PredicateCheck(string argumentName)
        {
            Contract.Requires(!string.IsNullOrEmpty(argumentName));
            _argumentName = argumentName;
        }

        public string ArgumentName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _argumentName;
            }
        }

        /// <summary>
        /// Returns true if current predicate checks specified <paramref name="name"/> for not-null.
        /// </summary>
        public bool ChecksForNotNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            return ArgumentName == name && ChecksForNotNull();
        }

        /// <summary>
        /// Returns true if curent predicate checks for not-null.
        /// </summary>
        /// <returns></returns>
        public abstract bool ChecksForNotNull();

        /// <summary>
        /// Returns true if current predicate checks specified <paramref name="name"/> for null.
        /// </summary>
        public bool ChecksForNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            return ArgumentName == name && ChecksForNull();
        }

        /// <summary>
        /// Returns true if current predicate contains null check.
        /// </summary>
        /// <returns></returns>
        public abstract bool ChecksForNull();
    }
}