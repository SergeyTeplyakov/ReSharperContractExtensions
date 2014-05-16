using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharper.ContractExtensions.Preconditions.Logic;
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
            var addAfter = GetPreviousInvariantStatement();

            AddStatementAfter(invariantMethod, invariantStatement, addAfter);
        }

        private void AddStatementAfter(IMethodDeclaration method, 
            ICSharpStatement statement, ICSharpStatement addAfter)
        {
            method.Body.AddStatementAfter(statement, addAfter);
        }

        [CanBeNull]
        private ICSharpStatement GetPreviousInvariantStatement()
        {

            var declaration = _invariantAvailability.FieldOrPropertyDeclaration;
            //if (declaration.IsField)
            //{
            //    return GetPreviousInvariantForField(declaration);
            //}

            //return GetPreviousInvariantForProperty(declaration);

            IEnumerable<IDeclaration> fields = _classDeclaration.MemberDeclarations.OfType<IFieldDeclaration>();
            IEnumerable<IDeclaration> properties = Enumerable.Empty<IDeclaration>();
            if (declaration.IsProperty)
            {
                properties = _classDeclaration.MemberDeclarations.OfType<IPropertyDeclaration>();
            }

            var members = fields.ToList();
            members.AddRange(properties);

            var membersInInvariants =
                _classDeclaration.GetInvariants()
                .Where(i => members.Any(f => i.ArgumentName == f.DeclaredName))
                .ToDictionary(x => x.ArgumentName, x => x);

            // Looking for "previous" members backwards: 
            foreach (var previousField in members.TakeWhile(fd => fd.DeclaredName != declaration.Name).Reverse())
            {
                if (membersInInvariants.ContainsKey(previousField.DeclaredName))
                {
                    return membersInInvariants[previousField.DeclaredName].Statement;
                }
            }

            return null;

            //var fields = _classDeclaration.MemberDeclarations.OfType<IFieldDeclaration>().ToList();

            //if (declaration.IsField)
            //{
            //    var index = fields.FindIndex(x => x.DeclaredName == declaration.Name);
            //    return index > 0 ? fields[index - 1] : null;

            //}
            //else
            //{
            //    var properties = _classDeclaration.MemberDeclarations.OfType<IPropertyDeclaration>().ToList();

            //    // If we're adding prop
            //    if (properties.Count == 0)
            //    {
            //        return fields.LastOrDefault();
            //    }

            //    var index = properties.FindIndex(x => x.DeclaredName == declaration.Name);
            //    return index > 0 ? properties[index - 1] : null;
        }

        private ICSharpStatement GetPreviousInvariantForProperty(FieldOrPropertyDeclaration declaration)
        {
            throw new System.NotImplementedException();
        }

        private ICSharpStatement GetPreviousInvariantForField(FieldOrPropertyDeclaration declaration)
        {
            var fields = _classDeclaration.MemberDeclarations.OfType<IFieldDeclaration>().ToList();

            var fieldInvariantStatements =
                _classDeclaration.GetInvariants()
                .Where(i => fields.Any(f => i.ArgumentName == f.DeclaredName))
                .ToDictionary(x => x.ArgumentName, x => x);

            // Looking for "previous" fields backwards
            foreach (var previousField in fields.SkipWhile(fd => fd.DeclaredName != declaration.Name).Reverse())
            {
                if (fieldInvariantStatements.ContainsKey(previousField.DeclaredName))
                {
                    return fieldInvariantStatements[previousField.DeclaredName].Statement;
                }
            }

            return null;
        }


        [CanBeNull]
        private IDeclaration GetPreviousFieldOrProperty(FieldOrPropertyDeclaration declaration)
        {
            var fields = _classDeclaration.MemberDeclarations.OfType<IFieldDeclaration>().ToList();

            if (declaration.IsField)
            {
                var index = fields.FindIndex(x => x.DeclaredName == declaration.Name);
                return index > 0 ? fields[index - 1] : null;

            }
            else
            {
                var properties = _classDeclaration.MemberDeclarations.OfType<IPropertyDeclaration>().ToList();

                // If we're adding prop
                if (properties.Count == 0)
                {
                    return fields.LastOrDefault();
                }

                var index = properties.FindIndex(x => x.DeclaredName == declaration.Name);
                return index > 0 ? properties[index - 1] : null;
            }
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
            ITypeElement type = TypeFactory.CreateTypeByCLRName(
                typeof(ContractInvariantMethodAttribute).FullName,
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