using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public abstract class PredicateCheck
    {
        private readonly PredicateArgument _argument;

        protected PredicateCheck(PredicateArgument argument)
        {
            Contract.Requires(argument != null);

            _argument = argument;
        }

        public PredicateArgument Argument
        {
            get
            {
                Contract.Ensures(Contract.Result<PredicateArgument>() != null);
                return _argument;
            }
        }

        /// <summary>
        /// Returns true if current predicate checks specified <paramref name="name"/> for not-null
        /// (like 'arg != null').
        /// </summary>
        public bool ChecksForNotNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            return _argument
                .With(x => x as ReferenceArgument)
                .With(x => x.ArgumentName) == name &&
                ChecksForNotNull();
        }

        /// <summary>
        /// Returns true if curent predicate checks for not-null.
        /// </summary>
        /// <returns></returns>
        public abstract bool ChecksForNotNull();

        /// <summary>
        /// Returns true if current predicate checks specified <paramref name="name"/> for null
        /// (like 'arg == null').
        /// </summary>
        public bool ChecksForNull(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            return _argument
                .With(x => x as ReferenceArgument)
                .With(x => x.ArgumentName) == name &&
                ChecksForNull();
        }

        /// <summary>
        /// Returns true if current predicate contains null check.
        /// </summary>
        /// <returns></returns>
        public abstract bool ChecksForNull();

        [CanBeNull]
        protected static PredicateArgument ExtractArgument(ICSharpExpression expression)
        {
            if (expression == null)
                return null;

            var contractResultArgument = TryCreateContractResultArgument(expression);
            if (contractResultArgument != null)
                return contractResultArgument;

            return TryCreateReferenceArgument(expression);
        }

        private static ContractResultPredicateArgument TryCreateContractResultArgument(
            ICSharpExpression expression)
        {
            // Specified expression could have Contract.Result as an inner expression, 
            // like Contract.Result<string>().Length
            // Let's try to find it out there.
            var contractResultReference =
                expression.ProcessRecursively<IInvocationExpression>()
                    .Select(ExtractContractResultReference)
                    .FirstOrDefault();

            if (contractResultReference == null)
                return null;

            return contractResultReference
                .With(x => x.TypeArguments.FirstOrDefault())
                .With(x => x as IDeclaredType)
                .Return(x => new ContractResultPredicateArgument(x, contractResultReference));
        }

        private static ReferenceArgument TryCreateReferenceArgument(ICSharpExpression expression)
        {
            return expression
                .With(x => x as IReferenceExpression)
                .Return(x => new ReferenceArgument(x));
        }

        [CanBeNull]
        private static IReferenceExpression ExtractContractResultReference(IInvocationExpression contractResultExpression)
        {
            Contract.Requires(contractResultExpression != null);

            var callSiteType = contractResultExpression.GetCallSiteType();
            var method = contractResultExpression.GetCalledMethod();

            if (callSiteType.With(x => x.FullName) != typeof(Contract).FullName ||
                method != "Result")
                return null;

            return contractResultExpression
                .With(x => x.InvokedExpression)
                .Return(x => x as IReferenceExpression);
        }
    }
}