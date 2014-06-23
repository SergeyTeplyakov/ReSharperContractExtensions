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

        protected ContractAssertionExpressionBase(AssertionType assertionType, string message)
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

    internal sealed class ContractRequiresExpression : ContractAssertionExpression
    {
        internal ContractRequiresExpression(AssertionType assertionType, List<PredicateCheck> predicates, IExpression predicateExpression, string message) 
            : base(assertionType, predicates, predicateExpression, message)
        {}

        [CanBeNull]
        public IClrTypeName GenericArgumentType { get; internal set; }
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
        private readonly List<PredicateCheck> _predicates;
        private readonly IExpression _predicateExpression;

        protected ContractAssertionExpression(AssertionType assertionType, List<PredicateCheck> predicates, 
            IExpression predicateExpression, string message)
            : base(assertionType, message)
        {
            Contract.Requires(predicates != null);
            Contract.Requires(predicateExpression != null);

            _predicates = predicates;
            _predicateExpression = predicateExpression;
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

            if (assertionType == AssertionType.Precondition)
            {
                var genericRequiresType = GetGenericRequiresType(invocationExpression);
                return new ContractRequiresExpression(assertionType.Value, predicates, originalExpression,
                    ExtractMessage(invocationExpression))
                {
                    GenericArgumentType = genericRequiresType,
                };
            }

            return new ContractAssertionExpression(assertionType.Value, predicates, originalExpression, 
                ExtractMessage(invocationExpression));
        }

        [System.Diagnostics.Contracts.Pure]
        private static IClrTypeName GetGenericRequiresType(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);
            var type = invocationExpression.Reference
                .With(x => x.Invocation)
                .With(x => x.TypeArguments.FirstOrDefault());

            return type.With(x => x as IDeclaredType)
                .With(x => x.GetClrName());
        }

        public ReadOnlyCollection<PredicateCheck> PreconditionExpressions
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<PredicateCheck>>() != null);
                return new ReadOnlyCollection<PredicateCheck>(_predicates);
            }
        }

        public IExpression PredicateExpression
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpression>() != null);
                return _predicateExpression;
            }
        }
    }
}