using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public static class ContractAssertionFactory
    {
        public static IList<ContractAssertion> FromFunctionDeclaration(
            ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);

            return functionDeclaration.Body
                .Return(x => x.Statements.AsEnumerable(), Enumerable.Empty<ICSharpStatement>())
                .Select(FromCSharpStatement)
                .ToList();
        }

        // TODO: ugly stuff!!
        [CanBeNull]
        public static ContractAssertion FromCSharpStatement(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invocationExpression = ContractAssertion.AsInvocationExpression(statement);
            if (invocationExpression == null)
                return null;

            var contractEnsureExpression = ContractEnsureExpression.FromInvocationExpression(invocationExpression);
            if (contractEnsureExpression != null)
                return new ContractEnsuresAssertion(statement, contractEnsureExpression);

            var assertion = ContractAssertionExpression.FromInvocationExpression(invocationExpression);
            if (assertion != null)
                return FromAssertionExpression(statement, assertion);

            return IfThrowPreconditionAssertion.TryCreate(statement);
        }

        private static ContractAssertion FromAssertionExpression(ICSharpStatement statement, ContractAssertionExpressionBase assertionExpression)
        {
            switch (assertionExpression.AssertionType)
            {
                case AssertionType.Precondition:
                    return new ContractRequiresPreconditionAssertion(statement,
                        (ContractAssertionExpression) assertionExpression);
                case AssertionType.Invariant:
                    return new ContractInvariantAssertion(statement,
                        (ContractAssertionExpression) assertionExpression);
                case AssertionType.Postcondition:
                case AssertionType.Assertion:
                case AssertionType.Assumption:
                    return null;
                    // TODO: assertion and assumption are not implemented yet!!
                default:
                    throw new InvalidOperationException("Unknown assertion type!");
            }
        }
    }
}