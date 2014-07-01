using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Markup;
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

            var ensuresReulstType = ExtractContractEnsuresResultType(expression as IInvocationExpression);

            if (ensuresReulstType != null)
                return new ContractResultPredicateArgument(ensuresReulstType);

            var referenceExpression =
                expression as IReferenceExpression;

            if (referenceExpression == null)
                return null;

            // The problem is, that for "person.Name != null" and
            // for "person != null" I should get "person"
            //IReferenceExpression qualifierReference = null;
            //    //referenceExpression.QualifierExpression
            //    //.With(x => x as IReferenceExpression);

            //string predicateArgument = (qualifierReference ?? referenceExpression).NameIdentifier.Name;

            //if (predicateArgument == null)
            //    return null;

            return new ReferenceArgument(referenceExpression);
        }

        [CanBeNull]
        private static IClrTypeName ExtractContractEnsuresResultType(IInvocationExpression contractResultExpression)
        {
            if (contractResultExpression == null)
                return null;

            var callSiteType = contractResultExpression.GetCallSiteType();
            var method = contractResultExpression.GetCalledMethod();

            if (callSiteType.With(x => x.FullName) != typeof(Contract).FullName ||
                method != "Result")
                return null;

            return contractResultExpression
                .With(x => x.InvokedExpression)
                .With(x => x as IReferenceExpression)
                .With(x => x.TypeArguments.FirstOrDefault())
                .With(x => x as IDeclaredType)
                .Return(x => x.GetClrName());
        }
    }
}