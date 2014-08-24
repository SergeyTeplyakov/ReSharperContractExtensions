using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractUtils;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions
{
    static class ContractStatementExtensions
    {
        /// <summary>
        /// Returns Code contract based preconditions only for specified <paramref name="functionDeclaration"/>.
        /// </summary>
        public static IEnumerable<ContractRequires> GetRequires(this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractRequires>>() != null);

            return functionDeclaration.GetPreconditions().OfType<ContractRequires>();
        }

        /// <summary>
        /// Returns all preconditions for specified <paramref name="functionDeclaration"/> including simple argument checks.
        /// </summary>
        public static IEnumerable<IPrecondition> GetPreconditions(this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<IPrecondition>>() != null);

            if (functionDeclaration.Body == null)
                return Enumerable.Empty<IPrecondition>();

            return functionDeclaration.Body.Statements
                .Select(ContractStatementFactory.TryCreatePrecondition).Where(p => p != null);
        }

        [CanBeNull]
        public static ContractRequires GetLastRequiresFor(this ICSharpFunctionDeclaration functionDeclaration,
            string parameterName)
        {
            var parameters = functionDeclaration.DeclaredElement.Parameters
                .Select(p => p.ShortName).TakeWhile(paramName => paramName != parameterName)
                .Reverse().ToList();

            // Creating lookup where key is argument name, and the value is statements.
            var requiresStatements =
                functionDeclaration
                    .GetRequires()
                    .ToList();
            /*.SelectMany(x => x.ArgumentNames.Select(a => new {Statement = x, ArgumentName = a}))
            .ToLookup(x => x.ArgumentName, x => x.Statement)*/
            ;

            // Looking for the last usage of the parameters in the requires statements
            foreach (var p in parameters)
            {
                // TODO: it seems terrible!!! and ugly!
                var precondition = requiresStatements
                    .LastOrDefault(r => r.ChecksForNotNull(pa =>
                        pa.CompareReferenceArgument(ra => ra.BaseArgumentName == p)));

                if (precondition != null)
                    return precondition;
            }

            return null;
        }

        public static IEnumerable<CodeContractAssertion> GetContractAssertions(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<CodeContractAssertion>>() != null);

            if (functionDeclaration.Body == null)
                return Enumerable.Empty<CodeContractAssertion>();

            Contract.Assert(functionDeclaration.Body != null);

            return functionDeclaration.Body.Statements
                .Select(ContractStatementFactory.TryCreateAssertion)
                .ToList();
        }

        public static IEnumerable<ContractEnsures> GetEnsures(this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractEnsures>>() != null);

            return GetContractAssertions(functionDeclaration).OfType<ContractEnsures>();
        }

        /// <summary>
        /// Return all invariant assertions (like Contract.Invariant(Prop != null)) for 
        /// the specified <paramref name="classLikeDeclaration"/>.
        /// </summary>
        public static IEnumerable<ContractInvariant> GetInvariants(this IClassLikeDeclaration classLikeDeclaration)
        {
            Contract.Requires(classLikeDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractInvariant>>() != null);

            return classLikeDeclaration.GetInvariantMethods().SelectMany(GetInvariants);
        }

        public static IEnumerable<ContractInvariant> GetInvariants(this ICSharpFunctionDeclaration invariantMethod)
        {
            Contract.Requires(invariantMethod != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractInvariant>>() != null);

            return GetContractAssertions(invariantMethod).OfType<ContractInvariant>();
        }
    }
}