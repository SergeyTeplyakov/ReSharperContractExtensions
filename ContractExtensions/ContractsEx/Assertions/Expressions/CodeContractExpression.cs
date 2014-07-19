using System;
using System.Collections.Generic;
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
    public interface ICodeContractExpression
    {
        AssertionType AssertionType { get; }
    }

    /// <summary>
    /// Represents one Assertion from Code Contract library, like Contract.Requires, Contract.Invariant etc.
    /// </summary>
    /// <remarks>
    /// Every valid precondtion contains following:
    /// Contract.Method(originalPredicates, message).
    /// 
    /// Note that this class is not suitable for Contract.Ensures because it has slightly 
    /// different internal structure.
    /// </remarks>
    public abstract class CodeContractExpression : ContractExpressionBase, ICodeContractExpression
    {
        private readonly IExpression _originalPredicateExpression;

        protected CodeContractExpression(IExpression originalPredicateExpression, 
            List<PredicateCheck> predicates, Message message)
            : base(predicates, message)
        {
            Contract.Requires(originalPredicateExpression != null);

            _originalPredicateExpression = originalPredicateExpression;
        }

        public IExpression OriginalPredicateExpression
        {
            get
            {
                Contract.Ensures(Contract.Result<IExpression>() != null);
                return _originalPredicateExpression;
            }
        }

        public abstract AssertionType AssertionType { get; }

        [CanBeNull]
        public static ICodeContractExpression FromInvocationExpression(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            AssertionType? assertionType = GetContractAssertionType(invocationExpression);
            if (assertionType == null)
                return null;

            if (assertionType == AssertionType.EndContractBlock)
                return new EndContractBlockExpression();

            IExpression originalPredicateExpression = invocationExpression.Arguments[0].Expression;

            var predicates = PredicateCheckFactory.Create(originalPredicateExpression).ToList();
            var message = ExtractMessage(invocationExpression);

            switch (assertionType.Value)
            {
                case AssertionType.Requires:
                    return new ContractRequiresExpression(invocationExpression, 
                        originalPredicateExpression, predicates, message);
                case AssertionType.Ensures:
                    return new ContractEnsuresExpression(originalPredicateExpression, predicates, message);
                case AssertionType.Invariant:
                    return new ContractInvariantExpression(originalPredicateExpression, predicates, message);
                case AssertionType.Assert:
                    return new ContractAssertExpression(originalPredicateExpression, predicates, message);
                case AssertionType.Assume:
                    return new ContractAssumeExpression(originalPredicateExpression, predicates, message);
                default:
                    Contract.Assert(false, "Unknown assertion type: " + assertionType.Value);
                    return null;
            }
        }

        private static AssertionType? GetContractAssertionType(IInvocationExpression invocationExpression)
        {
            var clrType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();

            if (clrType.Return(x => x.FullName) != typeof(Contract).FullName)
                return null;

            return ParseAssertionType(method);
        }

        private static AssertionType? ParseAssertionType(string method)
        {
            AssertionType result;
            if (Enum.TryParse(method, out result))
                return result;
            return null;
        }

        protected static Message ExtractMessage(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);
            Contract.Ensures(Contract.Result<Message>() != null);
            Contract.Assert(invocationExpression.Arguments.Count != 0);

            return invocationExpression.Arguments.Skip(1).FirstOrDefault()
                .With(x => x.Expression)
                .Return(Message.Create, NoMessage.Instance);
        }
    }
}