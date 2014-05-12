using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.Preconditions.Logic;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    public abstract class RequiresContextActionBase : ContextActionBase
    {
        protected readonly ICSharpContextActionDataProvider _provider;
        protected string _currentParameterName = "unknown";

        protected RequiresContextActionBase(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            _provider = provider;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var parameterDeclaration = GetParameterDeclaration();
            Contract.Assert(parameterDeclaration != null);

            var functionDeclaration = GetFunctionDeclaration(parameterDeclaration);

            var psiModule = parameterDeclaration.GetPsiModule();
            var factory = CSharpElementFactory.GetInstance(psiModule);

            ICSharpFile currentFile = (ICSharpFile)parameterDeclaration.GetContainingFile();
            Contract.Assert(currentFile != null);

            AddNamespaceUsing(factory, currentFile);
            var statement = CreateContractRequires(factory, parameterDeclaration);
            var addAfter = GetPreviousRequires(functionDeclaration, parameterDeclaration);

            functionDeclaration.Body.AddStatementAfter(statement, addAfter);

            return null;
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            IParameterDeclaration declaration;
            if (!IsParameterSelected(out declaration))
                return false;

            _currentParameterName = declaration.DeclaredElement.ShortName;

            return IsAvailableFor(declaration);
        }

        private IParameterDeclaration GetParameterDeclaration()
        {
            return _provider.GetSelectedElement<IParameterDeclaration>(true, true);
        }

        ICSharpStatement GetPreviousRequires(ICSharpFunctionDeclaration functionDeclaration,
            IParameterDeclaration parameterDeclaration)
        {
            var parameters = functionDeclaration.DeclaredElement.Parameters.Select(p => p.ShortName).ToList();
            var index = parameters.FindIndex(s => s == parameterDeclaration.DeclaredName);
            if (index == 0 || index == -1)
                return null;

            var prevParameterName = parameters[index - 1];
            var requiresStatements = functionDeclaration.GetRequires().ToList();

            return
                requiresStatements.FirstOrDefault(rs => rs.ArgumentName == prevParameterName)
                    .Return(x => x.Statement);
        }

        private bool IsParameterSelected(out IParameterDeclaration parameterDeclaration)
        {
            // If the carret is on the parameter declaration,
            // GetSelectedElement will return not-null value
            parameterDeclaration = _provider.GetSelectedElement<IParameterDeclaration>(true, true);
            if (parameterDeclaration != null)
                return true;

            // But parameter could be selected inside method body
            var selectedDeclaration = _provider.GetSelectedElement<IReferenceExpression>(true, true)
                .With(x => x.Reference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement)
                .With(x => x.GetDeclarations().FirstOrDefault())
                .Return(x => x as IParameterDeclaration);

            if (selectedDeclaration == null)
                return false;

            var currentFunction = _provider.GetSelectedElement<IFunctionDeclaration>(true, true);
            if (currentFunction == null)
                return false;

            bool isArgument =
                currentFunction.DeclaredElement.Parameters
                .SelectMany(pm => pm.GetDeclarations())
                .Select(pm => pm as IParameterDeclaration)
                .Where(pm => pm != null)
                .Contains(selectedDeclaration);

            if (isArgument)
            {
                parameterDeclaration = selectedDeclaration;
                return true;
            }

            return false;
        }

        protected abstract ICSharpStatement CreateContractRequires(CSharpElementFactory factory,
            IParameterDeclaration parameterDeclaration);

        private static void AddNamespaceUsing(CSharpElementFactory factory, ICSharpFile currentFile)
        {
            var diagnostics = factory.CreateUsingDirective("using $0", typeof(Contract).Namespace);
            if (!currentFile.Imports.ContainsUsing(diagnostics))
            {
                currentFile.AddImport(diagnostics);
            }
        }

        private bool IsAvailableFor(IParameterDeclaration declaration)
        {
            Contract.Requires(declaration != null);

            //Contract.Requires(declaration.IsValid());
            Contract.Requires(declaration.DeclaredElement != null);

            if (!IsCurrentFunctionIsWellDefined(declaration))
                return false;

            if (IsOutputParameter(declaration))
                return false;

            // For now, abstract methods are not supported
            if (MethodIsAbstract(declaration))
                return false;

            if (ArgumentIsDefaultedToNull(declaration))
                return false;

            if (ArgumentIsValueType(declaration) && !IsNullableValueType(declaration))
                return false;

            if (ArgumentIsAlreadyVerifiedByArgCheckOrRequires(declaration))
                return false;

            return true;
        }

        private bool IsCurrentFunctionIsWellDefined(IParameterDeclaration parameterDeclaration)
        {
            Contract.Requires(parameterDeclaration != null);

            var functionDeclaration = GetFunctionDeclaration(parameterDeclaration);
            return functionDeclaration != null && functionDeclaration.Body != null;
        }

        private bool IsOutputParameter(IParameterDeclaration declaration)
        {
            Contract.Requires(declaration != null);
            Contract.Requires(declaration.DeclaredElement != null);

            return declaration.DeclaredElement.Kind == ParameterKind.OUTPUT;
        }

        private bool ArgumentIsAlreadyVerifiedByArgCheckOrRequires(IParameterDeclaration parameterDeclaration)
        {
            var functionDeclaration = GetFunctionDeclaration(parameterDeclaration);

            var requiresStatements = functionDeclaration.GetRequires().ToList();

            return requiresStatements.Any(rs => rs.ArgumentName == parameterDeclaration.DeclaredName);
        }

        private bool ArgumentIsDefaultedToNull(IParameterDeclaration parameterDeclaration)
        {
            Contract.Requires(parameterDeclaration.DeclaredElement != null);

            var defaultValue = parameterDeclaration.DeclaredElement.GetDefaultValue();

            // TODO: not sure that GetDefaultValue could return null!
            return parameterDeclaration.DeclaredElement.IsOptional
                   && (defaultValue == null
                       || defaultValue.ConstantValue == null
                       || defaultValue.ConstantValue.Value == null);
        }

        private bool MethodIsAbstract(IParameterDeclaration parameterDeclaration)
        {
            var methodDeclaration = GetFunctionDeclaration(parameterDeclaration);
            // TODO: maybe would be useful to extract this as a state!
            return methodDeclaration.IsAbstract;
        }

        private bool IsNullableValueType(IParameterDeclaration parameterDeclaration)
        {
            return parameterDeclaration.Type.IsNullable();
        }

        private ICSharpFunctionDeclaration GetFunctionDeclaration(IParameterDeclaration parameterDeclaration)
        {
            // TODO question: is it possible that param.Parent == null or param.Parent.Parent == null?
            // TODO: Maybe we should search!! via _provider.Get
            Contract.Requires(parameterDeclaration != null);
            Contract.Ensures(Contract.Result<ICSharpFunctionDeclaration>() != null);

            Contract.Assert(parameterDeclaration.Parent != null, "Parameter should have a parent!");
            Contract.Assert(parameterDeclaration.Parent.Parent != null, "Paramter list should have a parent!");

            return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
            // This could fail!!
            //return (ICSharpFunctionDeclaration)parameterDeclaration.Parent.Parent;
        }

        private bool ArgumentIsValueType(IParameterDeclaration declaration)
        {
            Contract.Requires(declaration != null);
            Contract.Requires(declaration.DeclaredElement != null);

            var declaredElement = declaration.DeclaredElement;
            return declaredElement.Type.IsValueType() && !declaredElement.Type.IsNullable();
        }
    }
}