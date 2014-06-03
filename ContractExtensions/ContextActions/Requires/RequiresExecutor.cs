using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    internal sealed class RequiresExecutor
    {
        private readonly string _parameterName;

        private readonly ICSharpContextActionDataProvider _provider;
        private readonly bool _shouldBeGeneric;

        private readonly CSharpElementFactory _factory;
        private readonly ICSharpFile _currentFile;
        private readonly ICSharpFunctionDeclaration _functionDeclaration;

        public RequiresExecutor(ICSharpContextActionDataProvider provider, bool shouldBeGeneric, 
            ICSharpFunctionDeclaration functionDeclaration, string parameterName)
        {
            Contract.Requires(provider != null);
            Contract.Requires(functionDeclaration != null);
            Contract.Requires(parameterName != null);

            _provider = provider;
            _shouldBeGeneric = shouldBeGeneric;
            _functionDeclaration = functionDeclaration;
            _parameterName = parameterName;

            _factory = _provider.ElementFactory;
            _currentFile = (ICSharpFile)_functionDeclaration.GetContainingFile();

        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_parameterName != null);
            Contract.Invariant(_provider != null);
            Contract.Invariant(_currentFile != null);
            Contract.Invariant(_factory != null);
            Contract.Invariant(_functionDeclaration != null);
        }

        public void ExecuteTransaction()
        {
            AddUsingContractNamespaceIfNecessary();
            var statement = CreateContractRequires();
            var addAfter = GetPreviousRequires();

            _functionDeclaration.Body.AddStatementAfter(statement, addAfter);
        }

        private void AddUsingContractNamespaceIfNecessary()
        {
            var diagnostics = _factory.CreateUsingDirective("using $0", typeof(Contract).Namespace);
            if (!_currentFile.Imports.ContainsUsing(diagnostics))
            {
                _currentFile.AddImport(diagnostics);
            }
        }

        private ICSharpStatement CreateContractRequires()
        {
            string stringStatement;
            if (_shouldBeGeneric)
            {
                AddUsingArgumentNullExceptionsNamespaceIfNecessary();
                stringStatement = string.Format("{0}.Requires<ArgumentNullException>({1} != null);",
                    typeof(Contract).Name, _parameterName);
            }
            else
            {
                stringStatement = string.Format("{0}.Requires({1} != null);",
                    typeof (Contract).Name, _parameterName);
            }

            var statement = _factory.CreateStatement(stringStatement);
            return statement;
        }

        private void AddUsingArgumentNullExceptionsNamespaceIfNecessary()
        {
            var usingDirective = _factory.CreateUsingDirective("using $0", 
                typeof(ArgumentNullException).Namespace);

            if (!_currentFile.Imports.ContainsUsing(usingDirective))
            {
                _currentFile.AddImport(usingDirective);
            }
        }

        ICSharpStatement GetPreviousRequires()
        {
            // Getting all previous parameters in reverse order
            var parameters = _functionDeclaration.DeclaredElement.Parameters
                .Select(p => p.ShortName).TakeWhile(paramName => paramName != _parameterName)
                .Reverse().ToList();

            // Creating lookup where key is argument name, and the value is statements.
            var requiresStatements = 
                _functionDeclaration
                    .GetRequires()
                    .SelectMany(x => x.ArgumentNames.Select(a => new {Statement = x, ArgumentName = a}))
                    .ToLookup(x => x.ArgumentName, x => x.Statement);
            
            // Looking for the last usage of the parameters in the requires statements
            foreach (var p in parameters)
            {
                if (requiresStatements.Contains(p))
                {
                    return requiresStatements[p].Select(x => x.Statement).LastOrDefault();
                }
            }

            return null;
        }



    }
}