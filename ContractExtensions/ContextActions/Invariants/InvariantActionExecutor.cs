using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Invariants
{
    class InvariantActionExecutor
    {
        private readonly InvariantAvailability _invariantAvailability;
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly CSharpElementFactory _factory;
        private readonly ICSharpFile _currentFile;
        private readonly IClassLikeDeclaration _classDeclaration;

        public InvariantActionExecutor(InvariantAvailability invariantAvailability,
            ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(invariantAvailability != null);
            Contract.Requires(invariantAvailability.IsAvailable);
            Contract.Requires(provider != null);

            _invariantAvailability = invariantAvailability;
            _provider = provider;

            _factory = CSharpElementFactory.GetInstance(provider.PsiModule);
            // TODO: look at this class CSharpStatementNavigator

            _classDeclaration = provider.GetSelectedElement<IClassLikeDeclaration>(true, true);
            
            Contract.Assert(provider.SelectedElement != null);
            _currentFile = (ICSharpFile)provider.SelectedElement.GetContainingFile();
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_invariantAvailability != null);
            Contract.Invariant(_classDeclaration != null);
            Contract.Invariant(_currentFile != null);
            Contract.Invariant(_factory != null);
        }

        public void ExecuteTransaction(ISolution solution, IProgressIndicator progress)
        {
            AddNamespaceUsingIfNecessary();
            var invariantMethod = AddObjectInvariantMethodIfNecessary();

            var invariantStatement = CreateInvariantStatement();
            var addAfter = GetPreviousInvariantStatement(invariantMethod);

            AddStatementAfter(invariantMethod, invariantStatement, addAfter);
        }

        private void AddStatementAfter(IMethodDeclaration method, 
            ICSharpStatement statement, ICSharpStatement addAfter)
        {
            method.Body.AddStatementAfter(statement, addAfter);
        }

        private ICSharpStatement GetPreviousInvariantStatement(IMethodDeclaration invariantMethod)
        {
            Contract.Requires(invariantMethod != null);

            // TODO: will implement later!
            // Order will depends whether current member is field or property!
            return null;
        }

        private IMethodDeclaration AddObjectInvariantMethodIfNecessary()
        {
            Contract.Ensures(Contract.Result<IMethodDeclaration>() != null);
            Contract.Ensures(Contract.Result<IMethodDeclaration>().Body != null);

            var invariantMethod = _classDeclaration.GetInvariantMethod();
            if (invariantMethod == null)
                invariantMethod = CreateAndAddObjectInvariantMethod();

            if (!invariantMethod.IsObjectInvariantMethod())
                AddContractInvariantAttribute(invariantMethod);

            return invariantMethod;
        }

        private void AddContractInvariantAttribute(IMethodDeclaration method)
        {
            ITypeElement type = TypeFactory.CreateTypeByCLRName(typeof(ContractInvariantMethodAttribute).FullName,
                _provider.PsiModule, _currentFile.GetResolveContext()).GetTypeElement();

            var attribute = _factory.CreateAttribute(type);

            method.AddAttributeBefore(attribute, null);
        }

        private IMethodDeclaration CreateAndAddObjectInvariantMethod()
        {
            Contract.Ensures(Contract.Result<IMethodDeclaration>() != null);

            var method = (IMethodDeclaration)_factory.CreateTypeMemberDeclaration(
                string.Format("private void {0}() {{}}", InvariantUtils.InvariantMethodName),
                EmptyArray<object>.Instance);

            var lastConstructor = _classDeclaration.ConstructorDeclarations.LastOrDefault();

            _classDeclaration.AddClassMemberDeclarationAfter(method, lastConstructor);

            // To enable method modification, method from class declaration should be returned.
            return _classDeclaration.GetInvariantMethod();
        }

        private ICSharpStatement CreateInvariantStatement()
        {
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);

            string stringStatement = string.Format("{0}.Invariant({1} != null);",
                typeof(Contract).Name, _invariantAvailability.SelectedMemberName);
            var statement = _factory.CreateStatement(stringStatement);

            return statement;
        }

        private void AddNamespaceUsingIfNecessary()
        {
            var diagnostics = _factory.CreateUsingDirective("using $0", typeof(Contract).Namespace);
            if (!_currentFile.Imports.ContainsUsing(diagnostics))
            {
                _currentFile.AddImport(diagnostics);
            }
        }
    }
}