using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContractsEx.Assertions;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractsEx
{
    static class ContractStatementExtensions
    {
        /// <summary>
        /// Returns Code contract based preconditions only for specified <paramref name="functionDeclaration"/>.
        /// </summary>
        public static IEnumerable<ContractPreconditionStatementBase> GetContractPreconditions(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractPreconditionStatementBase>>() != null);

            return functionDeclaration.GetPreconditions().Where(p => p.IsCodeContractBasedPrecondition);
        }

        /// <summary>
        /// Returns all preconditions for specified <paramref name="functionDeclaration"/> including simple argument checks.
        /// </summary>
        public static IEnumerable<ContractPreconditionStatementBase> GetPreconditions(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractPreconditionStatementBase>>() != null);

            Contract.Assert(functionDeclaration.Body != null);
            return functionDeclaration.Body.Statements
                .Select(ContractPreconditionStatementBase.TryCreate).Where(p => p != null);
        }

        [System.Diagnostics.Contracts.Pure, CanBeNull]
        public static ContractPreconditionStatementBase GetLastPreconditionFor(this ICSharpFunctionDeclaration functionDeclaration, 
            string parameterName)
        {
            var parameters = functionDeclaration.DeclaredElement.Parameters
                .Select(p => p.ShortName).TakeWhile(paramName => paramName != parameterName)
                .Reverse().ToList();

            // Creating lookup where key is argument name, and the value is statements.
            var requiresStatements =
                functionDeclaration
                    .GetContractPreconditions()
                    .OfType<ContractRequiresStatement>()
                    .ToList();
            /*.SelectMany(x => x.ArgumentNames.Select(a => new {Statement = x, ArgumentName = a}))
            .ToLookup(x => x.ArgumentName, x => x.Statement)*/
            ;

            // Looking for the last usage of the parameters in the requires statements
            foreach (var p in parameters)
            {
                // TODO: it seems terrible!!! and ugly!
                var precondition = requiresStatements
                    .LastOrDefault(r => r.AssertsArgumentIsNotNull(pa => 
                        pa.CompareReferenceArgument(ra => ra.BaseArgumentName == p)));

                if (precondition != null)
                    return precondition;
            }

            return null;
        }

        public static IEnumerable<ContractStatementBase> GetContractStatements(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractStatementBase>>() != null);

            Contract.Assert(functionDeclaration.Body != null);

            return functionDeclaration.Body
                .Return(x => x.Statements.AsEnumerable(), Enumerable.Empty<ICSharpStatement>())
                .Select(ContractStatementFactory.FromCSharpStatement)
                .ToList();
        }

        public static IEnumerable<ContractEnsuresStatement> GetContractEnsures(
            this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractEnsuresStatement>>() != null);

            return GetContractStatements(functionDeclaration).OfType<ContractEnsuresStatement>();
        }

        /// <summary>
        /// Return all invariant assertions (like Contract.Invariant(Prop != null)) for 
        /// the specified <paramref name="classLikeDeclaration"/>.
        /// </summary>
        public static IEnumerable<ContractInvariantStatement> GetInvariantAssertions(this IClassLikeDeclaration classLikeDeclaration)
        {
            Contract.Requires(classLikeDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractInvariantStatement>>() != null);

            return classLikeDeclaration.GetInvariantMethods().SelectMany(GetInvariantAssertions);
        }

        public static IEnumerable<ContractInvariantStatement> GetInvariantAssertions(this ICSharpFunctionDeclaration invariantMethod)
        {
            Contract.Requires(invariantMethod != null);
            Contract.Ensures(Contract.Result<IEnumerable<ContractInvariantStatement>>() != null);

            return GetContractStatements(invariantMethod).OfType<ContractInvariantStatement>();
        }
    }
}