using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    public static class ContractStatementFactory
    {
        public static IList<ContractStatementBase> FromFunctionDeclaration(
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
        public static ContractStatementBase FromCSharpStatement(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var ifThrowExpression = IfThrowPreconditionExpression.FromStatement(statement);
            if (ifThrowExpression != null)
                return new IfThrowPreconditionStatement(statement, ifThrowExpression);

            var invocationExpression = ContractStatementBase.AsInvocationExpression(statement);
            if (invocationExpression == null)
                return null;

            var codeContractExpression = CodeContractExpression.FromInvocationExpression(invocationExpression);
            if (codeContractExpression != null)
                return CreateCodeContractStatement(statement, codeContractExpression);


            return null;
        }

        private static ContractStatementBase CreateCodeContractStatement(ICSharpStatement statement, 
            ICodeContractExpression codeContractExpression)
        {
            Contract.Requires(statement != null);
            Contract.Requires(codeContractExpression != null);
            Contract.Ensures(Contract.Result<ContractStatementBase>() != null);

            switch (codeContractExpression.AssertionType)
            {
                case AssertionType.Requires:
                    return new ContractRequiresStatement(statement, (ContractRequiresExpression)codeContractExpression);
                case AssertionType.Ensures:
                    return new ContractEnsuresStatement(statement, (ContractEnsuresExpression)codeContractExpression);
                case AssertionType.Invariant:
                    return new ContractInvariantStatement(statement, (ContractInvariantExpression)codeContractExpression);
                case AssertionType.Assert:
                    return new ContractAssertStatement(statement, (ContractAssertExpression)codeContractExpression);
                case AssertionType.Assume:
                    return new ContractAssumeStatement(statement, (ContractAssumeExpression)codeContractExpression);
                case AssertionType.EndContractBlock:
                    return new EndContractBlockStatement(statement, (EndContractBlockExpression)codeContractExpression);
                default:
                    Contract.Assert(false, "Unknown assertion type: " + codeContractExpression.AssertionType);
                    throw new InvalidOperationException("Unknown assertion type: " + 
                        codeContractExpression.AssertionType);
            }
        }
    }
}