using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.Settings;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    internal sealed class ArgumentRequiresExecutor
    {
        private readonly string _parameterName;
        private readonly IDeclaredType _propertyType;

        private readonly ICSharpContextActionDataProvider _provider;
        private readonly bool _shouldBeGeneric;

        private readonly CSharpElementFactory _factory;
        private readonly ICSharpFile _currentFile;
        private readonly ICSharpFunctionDeclaration _functionDeclaration;

        public ArgumentRequiresExecutor(ICSharpContextActionDataProvider provider, bool shouldBeGeneric, 
            ICSharpFunctionDeclaration functionDeclaration, string parameterName, IDeclaredType propertyType)
        {
            Contract.Requires(provider != null);
            Contract.Requires(functionDeclaration != null);
            Contract.Requires(parameterName != null);
            Contract.Requires(propertyType != null);

            _provider = provider;
            _shouldBeGeneric = shouldBeGeneric;
            _functionDeclaration = functionDeclaration;
            _parameterName = parameterName;
            _propertyType = propertyType;

            _factory = _provider.ElementFactory;
            _currentFile = (ICSharpFile)_functionDeclaration.GetContainingFile();

        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_parameterName != null);
            Contract.Invariant(_propertyType != null);
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

        [Pure]
        private ICSharpStatement CreateContractRequires()
        {
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);

            string compareStatement = CreateCompareStatement();

            string stringStatement;
            if (_shouldBeGeneric)
            {
                AddUsingArgumentNullExceptionsNamespaceIfNecessary();
                stringStatement = string.Format("{0}.Requires<ArgumentNullException>({1});",
                    typeof(Contract).Name, compareStatement);
            }
            else
            {
                stringStatement = string.Format("{0}.Requires({1});",
                    typeof (Contract).Name, compareStatement);
            }

            var statement = _factory.CreateStatement(stringStatement);
            return statement;
        }

        private string CreateCompareStatement()
        {
            if (_propertyType.GetClrName().FullName == typeof (string).FullName
                && ShouldCheckStringsForNullOrEmpty())
            {

                return string.Format("!string.IsNullOrEmpty({0})", _parameterName);
            }
            return string.Format("{0} != null", _parameterName);
        }

        private bool ShouldCheckStringsForNullOrEmpty()
        {
            var settings = _provider.SourceFile.GetSettingsStore()
                .GetKey<ContractExtensionsSettings>(SettingsOptimization.OptimizeDefault);

            return settings.CheckStringsForNullOrEmpty;
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