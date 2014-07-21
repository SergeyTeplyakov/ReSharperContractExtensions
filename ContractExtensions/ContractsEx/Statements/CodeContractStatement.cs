using System;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Statements
{
    /// <summary>
    /// List of supported code contract statements.
    /// </summary>
    public enum CodeContractStatementType
    {
        Requires,
        Ensures,
        EnsuresOnThrow,
        Assert,
        Assume,
        Invariant,
        EndContractBlock,
    }

    /// <summary>
    /// Represents lightweight version of code contract statement.
    /// </summary>
    public sealed class CodeContractStatement
    {
        private readonly ICSharpStatement _statement;
        private readonly IExpression _invokedExpression;
        private readonly CodeContractStatementType _statementType;

        private CodeContractStatement(ICSharpStatement statement, 
            IExpression invokedExpression,
            CodeContractStatementType statementType)
        {
            Contract.Requires(statement != null);

            _statement = statement;
            _invokedExpression = invokedExpression;
            _statementType = statementType;
        }

        public CodeContractStatementType StatementType
        {
            get { return _statementType; }
        }

        public bool IsPostcondition
        {
            get
            {
                return StatementType == CodeContractStatementType.Ensures ||
                       StatementType == CodeContractStatementType.EnsuresOnThrow;
            }
        }

        public bool IsPrecondition
        {
            get { return StatementType == CodeContractStatementType.Requires; }
        }

        public ICSharpStatement Statement
        {
            get { return _statement; }
        }

        [CanBeNull]
        public IExpression InvokedExpression
        {
            get { return _invokedExpression; }
        }

        public static CodeContractStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invocationExpression = ContractStatementBase.AsInvocationExpression(statement);
            if (invocationExpression == null)
                return null;

            var contractAssertion = GetContractAssertionType(invocationExpression);
            if (contractAssertion == null)
                return null;

            return new CodeContractStatement(statement, invocationExpression.InvokedExpression, contractAssertion.Value);
        }

        private static CodeContractStatementType? GetContractAssertionType(IInvocationExpression invocationExpression)
        {
            var clrType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();

            if (clrType.Return(x => x.FullName) != typeof(Contract).FullName)
                return null;

            return ParseCodeContractStatementType(method);
        }

        private static CodeContractStatementType? ParseCodeContractStatementType(string method)
        {
            CodeContractStatementType result;
            if (Enum.TryParse(method, out result))
                return result;
            return null;
        }
    }
}