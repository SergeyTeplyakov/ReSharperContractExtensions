using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.Preconditions.Logic;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions

{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class EnsuresContextAction : ContextActionBase
    {
        private const string MenuText = "Ensures result is not null";
        private const string Name = "Add Contract.Ensures";
        private const string Description = "Add Contract.Ensures on the potentially nullable return value.";

        private readonly ICSharpContextActionDataProvider _provider;

        public EnsuresContextAction(ICSharpContextActionDataProvider provider)
        {
            _provider = provider;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(IsAvailable(new UserDataHolder()));

            var functionDeclaration = GetFunctionDeclaration();

            var psiModule = functionDeclaration.GetPsiModule();
            var factory = CSharpElementFactory.GetInstance(psiModule);

            ICSharpFile currentFile = (ICSharpFile)functionDeclaration.GetContainingFile();
            Contract.Assert(currentFile != null);

            AddNamespaceUsing(factory, currentFile);
            var ensureStatement = CreateContractEnsures(factory, functionDeclaration);

            var requires = GetLastRequiresStatementIfAny(functionDeclaration);
            AddStatementAfter(functionDeclaration, ensureStatement, requires);

            return null;
        }

        public override string Text
        {
            get { return MenuText; }
        }

        [Pure]
        private bool IsPropertyWithGetterSelected()
        {
            var propertyDeclaration = _provider.GetSelectedElement<IPropertyDeclaration>(true, true);
            if (propertyDeclaration == null)
                return false;

            if (propertyDeclaration.IsAuto)
                return false;

            return propertyDeclaration.AccessorDeclarations.Any(a => a.Kind == AccessorKind.GETTER);
        }

        [Pure]
        private bool IsReturnOrMethodDeclarationSelected()
        {
            if (_provider.SelectedElement == null)
                return false;

            // Disable on parameters
            if (_provider.IsSelected<IParameterDeclaration>())
                return false;

            if (!_provider.IsSelected<IMethodDeclaration>())
                return false;

            // Enable on return statement
            if (_provider.IsSelected<IReturnStatement>())
                return true;

            // Ensures enable only on return statement in the method body
            if (_provider.IsSelected<IChameleonNode>())
                return false;

            return true;
        }

        private ICSharpFunctionDeclaration GetFunctionDeclaration()
        {
            Contract.Requires(IsReturnOrMethodDeclarationSelected() || IsPropertyWithGetterSelected());

            Contract.Ensures(Contract.Result<ICSharpFunctionDeclaration>() != null);
            Contract.Ensures(Contract.Result<ICSharpFunctionDeclaration>().DeclaredElement != null);

            if (IsReturnOrMethodDeclarationSelected())
                return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);

            if (IsPropertyWithGetterSelected())
            {
                return _provider.GetSelectedElement<IPropertyDeclaration>(true, true)
                    .AccessorDeclarations.First(d => d.Kind == AccessorKind.GETTER);
            }

            Contract.Assert(false, "Impossible situation");
            throw new InvalidOperationException();
        }

        [Pure]
        public override bool IsAvailable(IUserDataHolder cache)
        {
            if (!IsReturnOrMethodDeclarationSelected() && !IsPropertyWithGetterSelected())
                return false;

            var methodDeclaration = GetFunctionDeclaration();
            if (methodDeclaration == null || methodDeclaration.DeclaredElement == null)
                return false;

            if (ResultIsVoid(methodDeclaration) || MethodIsAbstract(methodDeclaration))
                return false;

            if (ResultIsAlreadyCheckedByContractEnsures(methodDeclaration))
                return false;

            if (methodDeclaration.GetReturnType().IsReferenceOrNullableType())
                return true;

            return false;
            
        }

        private ICSharpStatement GetLastRequiresStatementIfAny(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.GetRequires().Select(r => r.Statement).LastOrDefault();
        }

        private void AddStatementAfter(ICSharpFunctionDeclaration functionDeclaration, 
            ICSharpStatement statement, ICSharpStatement anchor)
        {
            functionDeclaration.Body.AddStatementAfter(statement, anchor);
        }

        private ICSharpStatement CreateContractEnsures(CSharpElementFactory factory, ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(factory != null);
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);
            // TODO: IsValid returns true even if resulting expression would not compiles!
            // To check this, please remove semicolon in the string below.
            // Maybe Resolve method would be good!
            // Contract.Ensures(Contract.Result<ICSharpStatement>().IsValid());

            Contract.Assert(IsAvailable(new UserDataHolder()));

            string statementLiteral = string.Format("{0}.Ensures(Contract.Result<{1}>() != null);",
                typeof (Contract).Name,
                functionDeclaration.DeclaredElement.ReturnType.GetPresentableName(CSharpLanguage.Instance));
            var result = factory.CreateStatement(statementLiteral);

            return result;
        }

        private bool ResultIsAlreadyCheckedByContractEnsures(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            if (functionDeclaration.Body == null)
                return false;
            
            var returnType = functionDeclaration.GetReturnType();
            return functionDeclaration.GetEnsures()
                .Any(e => e.ResultType.GetClrName().FullName == returnType.GetClrName().FullName);
        }

        private static void AddNamespaceUsing(CSharpElementFactory factory, ICSharpFile currentFile)
        {
            // TODO: how to express this in postcondition? Add another helper method for this!!
            var diagnostics = factory.CreateUsingDirective("using $0", typeof(Contract).Namespace);
            if (!currentFile.Imports.ContainsUsing(diagnostics))
            {
                currentFile.AddImport(diagnostics);
            }
        }

        private bool MethodIsAbstract(IFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.DeclaredElement.IsAbstract;
        }

        private bool ResultIsVoid(IFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.GetReturnType().IsVoid();
        }
    }
}