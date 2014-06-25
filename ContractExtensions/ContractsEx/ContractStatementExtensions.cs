using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.ContractUtils;

namespace ReSharper.ContractExtensions.ContractsEx
{
    static class ContractStatementExtensions
    {
        /// <summary>
        /// Returns Code contract based preconditions only for specified <paramref name="functionDeclaration"/>.
        /// </summary>
        public static IEnumerable<ContractPreconditionAssertion> GetContractPreconditions(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractPreconditionAssertion>>() != null);

            return functionDeclaration.GetPreconditions().Where(p => p.IsCodeContractBasedPrecondition);
        }

        /// <summary>
        /// Returns all preconditions for specified <paramref name="functionDeclaration"/> including simple argument checks.
        /// </summary>
        public static IEnumerable<ContractPreconditionAssertion> GetPreconditions(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractPreconditionAssertion>>() != null);

            Contract.Assert(functionDeclaration.Body != null);
            return functionDeclaration.Body.Statements
                .Select(ContractPreconditionAssertion.TryCreate).Where(p => p != null);
        }

        [System.Diagnostics.Contracts.Pure, CanBeNull]
        public static ContractPreconditionAssertion GetLastPreconditionFor(this ICSharpFunctionDeclaration functionDeclaration, 
            string parameterName)
        {
            var parameters = functionDeclaration.DeclaredElement.Parameters
                .Select(p => p.ShortName).TakeWhile(paramName => paramName != parameterName)
                .Reverse().ToList();

            // Creating lookup where key is argument name, and the value is statements.
            var requiresStatements =
                functionDeclaration
                    .GetContractPreconditions().ToList();
            /*.SelectMany(x => x.ArgumentNames.Select(a => new {Statement = x, ArgumentName = a}))
            .ToLookup(x => x.ArgumentName, x => x.Statement)*/
            ;

            // Looking for the last usage of the parameters in the requires statements
            foreach (var p in parameters)
            {
                var precondition = requiresStatements.LastOrDefault(r => r.ChecksForNull(p));
                if (precondition != null)
                    return precondition;
            }

            return null;
        }

        public static IEnumerable<ContractEnsuresAssertion> GetContractEnsures(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractEnsuresAssertion>>() != null);

            Contract.Assert(functionDeclaration.Body != null);

            return functionDeclaration.Body.Statements
                .Select(ContractEnsuresAssertion.TryCreate)
                .Where(a => a != null);
        }

        /// <summary>
        /// Return all invariant assertions (like Contract.Invariant(Prop != null)) for 
        /// the specified <paramref name="classLikeDeclaration"/>.
        /// </summary>
        public static IEnumerable<ContractInvariantAssertion> GetInvariantAssertions(this IClassLikeDeclaration classLikeDeclaration)
        {
            Contract.Requires(classLikeDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractInvariantAssertion>>() != null);

            return classLikeDeclaration.GetInvariantMethods().SelectMany(GetInvariantAssertions);
        }

        public static IEnumerable<ContractInvariantAssertion> GetInvariantAssertions(this ICSharpFunctionDeclaration invariantMethod)
        {
            Contract.Requires(invariantMethod != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractInvariantAssertion>>() != null);

            Contract.Assert(invariantMethod.Body != null);

            return invariantMethod.Body.Statements
                .Select(ContractInvariantAssertion.TryCreate)
                .Where(s => s != null);
        }
    }
}