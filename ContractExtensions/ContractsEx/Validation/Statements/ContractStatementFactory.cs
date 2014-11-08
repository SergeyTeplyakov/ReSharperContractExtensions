using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers.MalformContractAnalyzers;

namespace ReSharper.ContractExtensions.ContractsEx.Assertions.Statements
{
    public static class ContractStatementFactory
    {
        private static readonly List<Func<ICSharpStatement, ContractStatement>> _factoryMethods = GetFactoryMethods().ToList();

        private static IEnumerable<Func<ICSharpStatement, ContractStatement>> GetFactoryMethods()
        {
            yield return CodeContractStatement.TryCreate;
            yield return CreateSpecialInvocationContractStatement;
            yield return IfThrowStatement.TryCreate;
        }

        [CanBeNull]
        public static ContractStatement TryCreate(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            return 
                _factoryMethods
                .Select(factory => factory(statement))
                .FirstOrDefault(contract => contract != null);
        }

        [CanBeNull]
        private static ContractStatement CreateSpecialInvocationContractStatement(ICSharpStatement statement)
        {
            var method = statement.GetInvokedMethod();

            if (method == null)
                return null;

            var validatorAttribute = new ClrTypeName("System.Diagnostics.Contracts.ContractArgumentValidatorAttribute");
            if (method.HasAttributeInstance(validatorAttribute, false))
                return ContractValidatorStatement.TryCreate(statement, method);

            var abbreviationAttribute = new ClrTypeName("System.Diagnostics.Contracts.ContractAbbreviatorAttribute");
            if (method.HasAttributeInstance(abbreviationAttribute, false))
                return ContractAbbreviationStatement.TryCreate(statement, method);

            return null;
        }
    }
}