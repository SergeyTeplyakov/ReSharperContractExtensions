using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.Preconditions.Logic
{
    static class ContractStatementExtensions
    {
        public static IEnumerable<RequiresStatement> GetRequires(this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<RequiresStatement>>() != null);

            Contract.Assert(functionDeclaration.Body != null);

            return functionDeclaration.Body.Statements
                .Select(RequiresStatement.TryCreate).Where(rs => rs != null);
        }

        public static IEnumerable<EnsuresStatement> GetEnsures(this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<EnsuresStatement>>() != null);
            
            Contract.Assert(functionDeclaration.Body != null);

            return functionDeclaration.Body.Statements
                .Select(EnsuresStatement.TryCreate).Where(rs => rs != null);
        }

        public static IEnumerable<InvariantStatement> GetInvariants(this ICSharpFunctionDeclaration invariantMethod)
        {
            Contract.Requires(invariantMethod != null);
            Contract.Ensures(Contract.Result<IEnumerable<InvariantStatement>>() != null);

            Contract.Assert(invariantMethod.Body != null);

            return invariantMethod.Body.Statements
                .Select(InvariantStatement.TryCreate)
                .Where(s => s != null);
        }
    }
}