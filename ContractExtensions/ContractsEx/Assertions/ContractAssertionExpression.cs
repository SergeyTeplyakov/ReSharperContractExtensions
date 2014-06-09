using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    /// <summary>
    /// Base class for every contract expressions like Contract.Requires, Contract.Ensures etc.
    /// </summary>
    internal abstract class ContractAssertionExpressionBase
    {
        private readonly AssertionType _assertionType;
        private readonly string _message;

        protected ContractAssertionExpressionBase(AssertionType assertionType, [NotNull] string message)
        {
            _assertionType = assertionType;
            _message = message;
        }

        public AssertionType AssertionType
        {
            get { return _assertionType; }
        }

        [CanBeNull]
        public string Message
        {
            get { return _message; }
        }

        protected static AssertionType? GetAssertionType(IInvocationExpression invocationExpression)
        {
            var clrType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();

            if (clrType.Return(x => x.FullName) != typeof (Contract).FullName)
                return null;

            return ParseAssertionType(method);
        }

        private static AssertionType? ParseAssertionType(string method)
        {
            switch (method)
            {
                case "Requires":
                    return AssertionType.Precondition;
                case "Ensures":
                    return AssertionType.Postcondition;
                case "Assert":
                    return AssertionType.Assertion;
                case "Invariant":
                    return AssertionType.Invariant;
                case "Assume":
                    return AssertionType.Assumption;
                default:
                    return null;

            }
        }

        protected static string ExtractMessage(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            Contract.Assert(invocationExpression.Arguments.Count != 0);

            var message = invocationExpression.Arguments.Skip(1).FirstOrDefault()
                .With(x => x.Expression as ICSharpLiteralExpression)
                .With(x => x.Literal.GetText());
            return message;
        }
    }

    /// <summary>
    /// Represents one Assertion from Code Contract library, like Contract.Requires, Contract.Invariant etc.
    /// </summary>
    /// <remarks>
    /// Every valid precondtion contains following:
    /// Contract.Method(predicates, message).
    /// 
    /// Note that this class is not suitable for Contract.Ensures because it has slightly 
    /// different internal structure.
    /// </remarks>
    internal class ContractAssertionExpression : ContractAssertionExpressionBase
    {
        private readonly List<IPredicateCheck> _predicates;

        private ContractAssertionExpression(AssertionType assertionType, List<IPredicateCheck> predicates, string message)
            : base(assertionType, message)
        {
            Contract.Requires(predicates != null);

            _predicates = predicates;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_predicates != null);
            Contract.Invariant(_predicates.Count != 0);
        }
        
        [CanBeNull]
        public static ContractAssertionExpression FromInvocationExpression(
            IInvocationExpression invocationExpression)
        {
            // TODO: potential enhancement: simplify condition first and convert !(result == null)
            Contract.Requires(invocationExpression != null);

            AssertionType? assertionType = GetAssertionType(invocationExpression);
            if (assertionType == null)
                return null;

            Contract.Assert(invocationExpression.Arguments.Count != 0,
                "Precondition expression should have at least one argument!");

            IExpression originalExpression = invocationExpression.Arguments[0].Expression;

            var predicates = PredicateCheckFactory.Create(originalExpression).ToList();

            if (predicates.Count == 0)
                return null;

            return new ContractAssertionExpression(assertionType.Value, predicates, 
            ExtractMessage(invocationExpression));
        }

        public ReadOnlyCollection<IPredicateCheck> PreconditionExpressions
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<IPredicateCheck>>() != null);
                return new ReadOnlyCollection<IPredicateCheck>(_predicates);
            }
        }
    }

    internal sealed class ContractEnsureExpression : ContractAssertionExpressionBase
    {
        public ContractEnsureExpression(IDeclaredType resultType, string message) 
            : base(AssertionType.Postcondition, message)
        {
            Contract.Requires(resultType != null);

            ResultType = resultType;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(ResultType != null);
        }

        [CanBeNull]
        public static ContractEnsureExpression FromInvocationExpression(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            AssertionType? assertionType = GetAssertionType(invocationExpression);
            if (assertionType == null || assertionType != AssertionType.Postcondition)
                return null;

            Contract.Assert(invocationExpression.Arguments.Count != 0,
                "Precondition expression should have at least one argument!");

            IExpression originalExpression = invocationExpression.Arguments[0].Expression;

            var expression = originalExpression as IEqualityExpression;
            if (expression == null)
                return null;

            // looking for type from expression like: Contract.Result<Type>()
            var ensureType = ExtractContractResultType(
                expression.LeftOperand.With(x => x as IInvocationExpression));

            var right = expression.RightOperand
                .With(x => x as ICSharpLiteralExpression)
                .With(x => x.Literal)
                .Return(x => x.GetText());

            if (ensureType == null || right == null || right != "null" &&
                expression.EqualityType != EqualityExpressionType.NE)
            {
                return null;
            }

            return new ContractEnsureExpression(ensureType, ExtractMessage(invocationExpression));
        }

        public IDeclaredType ResultType { get; private set; }

        [CanBeNull]
        private static IDeclaredType ExtractContractResultType(IInvocationExpression contractResultExpression)
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
                .Return(x => x as IDeclaredType);
        }
    }
}