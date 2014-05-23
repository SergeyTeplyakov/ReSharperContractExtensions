using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Feature.Services.Html;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using ReSharper.ContractExtensions.ContextActions.ContractsFor;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Shows whether "Add Requires" action is available or not.
    /// </summary>
    public sealed class RequiresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly IParameterDeclaration _parameterDeclaration;
        private readonly ICSharpFunctionDeclaration _functionDeclaration;

        public readonly static RequiresAvailability Unavailable = new RequiresAvailability {IsAvailable = false};

        private RequiresAvailability()
        {}

        public RequiresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            _provider = provider;

            _parameterDeclaration = GetSelectedParameterDeclaration();
            _functionDeclaration = GetSelectedFunctionDeclaration();
            IsAvailable = IsActionAvailable();
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _parameterDeclaration != null);
            Contract.Invariant(!IsAvailable || _parameterDeclaration.DeclaredElement != null);
            Contract.Invariant(!IsAvailable || SelectedParameter != null);
            Contract.Invariant(!IsAvailable || !SelectedParameterName.IsNullOrEmpty());
            Contract.Invariant(!IsAvailable || _functionDeclaration != null);
            Contract.Invariant(!IsAvailable || FunctionToInsertPrecondition != null);
        }

        public static RequiresAvailability Create(ICSharpContextActionDataProvider provider)
        {
            return new RequiresAvailability(provider);
        }

        public bool IsAvailable { get; private set; }
        public IParameterDeclaration SelectedParameter { get { return _parameterDeclaration; } }
        public ICSharpFunctionDeclaration FunctionDeclaration { get { return _functionDeclaration; } }
        public ICSharpFunctionDeclaration FunctionToInsertPrecondition { get; private set; }
        public string SelectedParameterName { get { return _parameterDeclaration.DeclaredName; } }

        private IParameterDeclaration GetSelectedParameterDeclaration()
        {
            // If the carret is on the parameter _parameterDeclaration,
            // GetSelectedElement will return not-null value
            var parameterDeclaration = _provider.GetSelectedElement<IParameterDeclaration>(true, true);
            if (parameterDeclaration != null)
                return parameterDeclaration;

            // But parameter could be selected inside method body
            var selectedDeclaration = _provider.GetSelectedElement<IReferenceExpression>(true, true)
                .With(x => x.Reference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement)
                .With(x => x.GetDeclarations().FirstOrDefault())
                .Return(x => x as IParameterDeclaration);

            
            if (selectedDeclaration == null)
                return null;

            var currentFunction = 
                _provider.GetSelectedElement<IFunctionDeclaration>(true, true);

            if (currentFunction == null)
                return null;

            bool isArgument =
                currentFunction.DeclaredElement.Parameters
                .SelectMany(pm => pm.GetDeclarations())
                .Select(pm => pm as IParameterDeclaration)
                .Where(pm => pm != null)
                .Contains(selectedDeclaration);

            return isArgument ? selectedDeclaration : null;
        }

        private ICSharpFunctionDeclaration GetSelectedFunctionDeclaration()
        {
            return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
        }

        private bool IsActionAvailable()
        {
            if (!IsParameterDeclarationWellDefined())
                return false;

            if (IsOutputParameter())
                return false;

            if (ArgumentIsDefaultedToNull())
                return false;

            if (ParameterIsValueType() && !IsNullableValueType())
                return false;

            var functionToInsertPrecondition = _functionDeclaration.GetContractFunction();

            if (!IsFunctionWellDefined(functionToInsertPrecondition))
                return false;

            if (ArgumentIsAlreadyVerifiedByArgCheckOrRequires(functionToInsertPrecondition))
                return false;

            FunctionToInsertPrecondition = functionToInsertPrecondition;
            return true;
        }

        [Pure]
        private bool IsParameterDeclarationWellDefined()
        {
            return _parameterDeclaration != null && _parameterDeclaration.DeclaredElement != null;
        }

        [Pure]
        private bool IsFunctionWellDefined(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration != null 
                && functionDeclaration.Body != null 
                && functionDeclaration.DeclaredElement != null;
        }

        [System.Diagnostics.Contracts.Pure]
        private bool IsOutputParameter()
        {
            return _parameterDeclaration.DeclaredElement.Kind == ParameterKind.OUTPUT;
        }

        [Pure]
        private bool ArgumentIsAlreadyVerifiedByArgCheckOrRequires(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);

            var requiresStatements = functionDeclaration.GetRequires().ToList();

            return requiresStatements.Any(rs => rs.ArgumentName == _parameterDeclaration.DeclaredName);
        }

        [Pure]
        private bool ArgumentIsDefaultedToNull()
        {
            var defaultValue = _parameterDeclaration.DeclaredElement.GetDefaultValue();

            // TODO: not sure that GetDefaultValue could return null!
            return _parameterDeclaration.DeclaredElement.IsOptional
                   && (defaultValue == null
                       || defaultValue.ConstantValue == null
                       || defaultValue.ConstantValue.Value == null);
        }

        [Pure]
        private bool IsNullableValueType()
        {
            return _parameterDeclaration.Type.IsNullable();
        }

        [Pure]
        private bool ParameterIsValueType()
        {
            var declaredElement = _parameterDeclaration.DeclaredElement;
            return declaredElement.Type.IsValueType() && !declaredElement.Type.IsNullable();
        }

    }
}