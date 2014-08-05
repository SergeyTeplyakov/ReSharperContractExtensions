using System.Diagnostics.Contracts;
using System;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util.Special;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    /// <summary>
    /// Represents argument for the <seealso cref="PredicateCheck"/>.
    /// </summary>
    public abstract class PredicateArgument 
    {}

    /// <summary>
    /// Represents an absence of the argument (this is some kind of Null Object Pattern).
    /// </summary>
    public sealed class EmptyPredicateArgument : PredicateArgument
    { }

    /// <summary>
    /// Represents "reference" argument that contains a name for the argument/field/property.
    /// </summary>
    public sealed class ReferenceArgument : PredicateArgument
    {
        private readonly IReferenceExpression _referenceExpression;
        private readonly string _argumentName;
        private readonly string _baseArgumentName;

        public ReferenceArgument(IReferenceExpression referenceExpression)
        {
            Contract.Requires(referenceExpression != null);
            Contract.Requires(referenceExpression.NameIdentifier != null);
            _referenceExpression = referenceExpression;

            _argumentName = referenceExpression.NameIdentifier.Name;

            var qualifierReference = referenceExpression.QualifierExpression as IReferenceExpression;

            _baseArgumentName = (qualifierReference ?? referenceExpression).NameIdentifier.Name;
        }

        /// <summary>
        /// Returns argument name itself. For 'person.Foo' reference expression, ArgumentName woul be 'Foo'.
        /// </summary>
        public string ArgumentName
        {
            get { return _argumentName; }
        }

        /// <summary>
        /// Returns "base name" of the argment. I.e. for 'person.Foo' reference expression, BaseArgumentName
        /// will return 'person'.
        /// </summary>
        /// <returns></returns>
        public string BaseArgumentName
        {
            get { return _baseArgumentName; }
        }

        public IReferenceExpression ReferenceExpression
        {
            get { return _referenceExpression; }
        }
    }

    /// <summary>
    /// Represents Contract.Result&lt;T&gt; predicate argument.
    /// </summary>
    public sealed class ContractResultPredicateArgument : PredicateArgument
    {
        private readonly IDeclaredType _resultTypeName;
        private readonly IReferenceExpression _contractResultReference;

        public ContractResultPredicateArgument(IDeclaredType resultTypeName, 
            IReferenceExpression contractResultReference)
        {
            Contract.Requires(resultTypeName != null);
            Contract.Requires(contractResultReference != null);

            _resultTypeName = resultTypeName;
            _contractResultReference = contractResultReference;
        }

        public IDeclaredType ResultType
        {
            get { return _resultTypeName; }
        }

        public void SetResultType(IType contractResultType)
        {
            Contract.Requires(contractResultType != null);

            _contractResultReference.SetTypeArguments(new[] {contractResultType});
        }

        public IClrTypeName ResultTypeName
        {
            get { return _resultTypeName.GetClrName(); }
        }
    }

    public static class PredicateArgumentEx
    {
        public static bool CompareReferenceArgument(this PredicateArgument argument,
            Func<ReferenceArgument, bool> comparer)
        {
            Contract.Requires(argument != null);
            Contract.Requires(comparer != null);

            var stringArgument = argument as ReferenceArgument;

            return stringArgument != null && comparer(stringArgument);
        }

        public static bool CompareContractResultArgument(this PredicateArgument argument,
            Func<ContractResultPredicateArgument, bool> comparer)
        {
            Contract.Requires(argument != null);
            Contract.Requires(comparer != null);

            var contractResultPredicateArgument = argument as ContractResultPredicateArgument;
            Contract.Assert(contractResultPredicateArgument != null);

            return comparer(contractResultPredicateArgument);
        }
    }

}