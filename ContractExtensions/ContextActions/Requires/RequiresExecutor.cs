using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Features.SolBuilderDuo.Engine.MsbuildExe.Components;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.Preconditions.Logic;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    internal sealed class RequiresExecutor
    {
        private readonly RequiresAvailability _requiresAvailability;
        private readonly string _parameterName;

        private readonly ICSharpContextActionDataProvider _provider;
        private readonly bool _shouldBeGeneric;

        private readonly CSharpElementFactory _factory;
        private readonly ICSharpFile _currentFile;
        private readonly ICSharpFunctionDeclaration _functionDeclaration;

        public RequiresExecutor(RequiresAvailability requiresAvailability, 
            ICSharpContextActionDataProvider provider, bool shouldBeGeneric)
        {
            Contract.Requires(requiresAvailability != null);
            Contract.Requires(requiresAvailability.IsAvailable);
            Contract.Requires(provider != null);

            _requiresAvailability = requiresAvailability;
            _provider = provider;
            _shouldBeGeneric = shouldBeGeneric;
            _parameterName = _requiresAvailability.SelectedParameterName;

            _functionDeclaration = _requiresAvailability.FunctionDeclaration;
            _factory = CSharpElementFactory.GetInstance(provider.PsiModule);
            _currentFile = (ICSharpFile)requiresAvailability.SelectedParameter.GetContainingFile();
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_requiresAvailability != null);
            Contract.Invariant(_provider != null);
            Contract.Invariant(_currentFile != null);
            Contract.Invariant(_factory != null);
        }

        public void ExecuteTransaction(ISolution solution, IProgressIndicator progress)
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
            var parameters = _functionDeclaration.DeclaredElement.Parameters.Select(p => p.ShortName).ToList();
            var index = parameters.FindIndex(s => s == _parameterName);
            if (index == 0 || index == -1)
                return null;

            var prevParameterName = parameters[index - 1];
            var requiresStatements = _functionDeclaration.GetRequires().ToList();

            return
                requiresStatements.FirstOrDefault(rs => rs.ArgumentName == prevParameterName)
                    .Return(x => x.Statement);
        }



    }
}