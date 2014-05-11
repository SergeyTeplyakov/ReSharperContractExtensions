using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.Preconditions.Logic
{
    /// <summary>
    /// Represent entire Contract.Requires statement.
    /// </summary>
    internal sealed class EnsuresStatement : ContractStatement
    {
        private EnsuresStatement(ICSharpStatement statement)
            : base(statement)
        {}

        public static EnsuresStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            var invocationExpression = AsInvocationExpression(statement);
            if (invocationExpression == null)
                return null;

            if (!IsContractEnsures(invocationExpression))
                return null;

            var ensureExpression = EnsureExpression.Parse(invocationExpression.Arguments[0].Expression);
            if (!ensureExpression.IsValid)
                return null;

            var result = new EnsuresStatement(statement);
            result.ResultType = ensureExpression.ResultType;
            result.Message = ExtractMessage(invocationExpression);

            return result;
        }

        public IDeclaredType ResultType { get; private set; }
        public string Message { get; private set; }

        private static bool IsContractEnsures(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);
            // If result is true, then invocation expression should have at least one argument
            Contract.Ensures(Contract.Result<bool>() == false || invocationExpression.Arguments.Count > 0);

            var clrType = invocationExpression.GetCallSiteType();
            var method = invocationExpression.GetCalledMethod();

            return clrType.Return(x => x.FullName) == typeof (Contract).FullName 
                && method == "Ensures";
        }

        private static string ExtractMessage(IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            Contract.Assert(invocationExpression.Arguments.Count != 0);

            var message = invocationExpression.Arguments.Skip(1).FirstOrDefault()
                .With(x => x.Expression as ICSharpLiteralExpression)
                .With(x => x.Literal.GetText());
            return message;
        }
    }
}