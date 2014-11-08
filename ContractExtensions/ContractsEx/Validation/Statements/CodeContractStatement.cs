using System;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Statements
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
    public sealed class CodeContractStatement : ContractStatement
    {
        /// <summary>
        /// Represents "invocation" part of the code contract statement.
        /// For example, for Contract.Requies(s != null) invocationExpression would be "s != null"
        /// </summary>
        private readonly IInvocationExpression _invocationExpression;
        private readonly CodeContractStatementType _statementType;
        private JetBrains.Util.Lazy.Lazy<CodeContractAssertion> _codeContractExpression;

        private CodeContractStatement(ICSharpStatement statement, 
            IInvocationExpression invocationExpression,
            CodeContractStatementType statementType)
            : base(statement)
        {
            Contract.Requires(invocationExpression != null);

            _invocationExpression = invocationExpression;
            _statementType = statementType;

            // Due to weird bug in CC compiler, I can't use the same variable in Contract.Requires
            // and in lambda expression.
            _codeContractExpression = JetBrains.Util.Lazy.Lazy.Of(
                () => Assertions.ContractStatementFactory.TryCreateAssertion(_invocationExpression));
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

        public JetBrains.Util.Lazy.Lazy<CodeContractAssertion> CodeContractExpression
        {
            get
            {
                return _codeContractExpression;
            }
        }

        public bool IsEndContractBlock
        {
            get { return StatementType == CodeContractStatementType.EndContractBlock; }
        }

        public bool IsMethodContractStatement
        {
            get { return IsPrecondition || IsPostcondition || IsEndContractBlock; }
        }

        public override bool IsPrecondition
        {
            get { return StatementType == CodeContractStatementType.Requires; }
        }

        public ICSharpFunctionDeclaration GetDeclaredMethod()
        {
            Contract.Ensures(Contract.Result<ICSharpFunctionDeclaration>() != null);
            return _statement.GetContainingNode<ICSharpFunctionDeclaration>();
        }

        public IInvocationExpression InvocationExpression
        {
            get
            {
                Contract.Ensures(Contract.Result<IInvocationExpression>() != null);
                return _invocationExpression;
            }
        }

        internal static CodeContractStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invocationExpression = Impl.StatementUtils.AsInvocationExpression(statement);
            if (invocationExpression == null)
                return null;

            var contractAssertion = GetContractAssertionType(invocationExpression);
            if (contractAssertion == null)
                return null;

            return new CodeContractStatement(statement, invocationExpression, contractAssertion.Value);
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