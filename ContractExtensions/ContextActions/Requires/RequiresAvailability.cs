using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using ReSharper.ContractExtensions.Preconditions.Logic;
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
        }

        public static RequiresAvailability Create(ICSharpContextActionDataProvider provider)
        {
            return new RequiresAvailability(provider);
        }

        public bool IsAvailable { get; private set; }
        public IParameterDeclaration SelectedParameter { get { return _parameterDeclaration; } }
        public ICSharpFunctionDeclaration FunctionDeclaration { get { return _functionDeclaration; } }
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

            if (!IsCurrentFunctionWellDefined())
                return false;

            if (IsOutputParameter())
                return false;

            // For now, abstract methods are not supported
            if (MethodIsAbstract())
                return false;

            if (ArgumentIsDefaultedToNull())
                return false;

            if (ParameterIsValueType() && !IsNullableValueType())
                return false;

            if (ArgumentIsAlreadyVerifiedByArgCheckOrRequires())
                return false;

            return true;
        }

        private bool IsParameterDeclarationWellDefined()
        {
            return _parameterDeclaration != null && _parameterDeclaration.DeclaredElement != null;
        }

        private bool IsCurrentFunctionWellDefined()
        {
            return _functionDeclaration != null && _functionDeclaration.Body != null;
        }

        private bool IsOutputParameter()
        {
            return _parameterDeclaration.DeclaredElement.Kind == ParameterKind.OUTPUT;
        }

        private bool ArgumentIsAlreadyVerifiedByArgCheckOrRequires()
        {
            var requiresStatements = _functionDeclaration.GetRequires().ToList();

            return requiresStatements.Any(rs => rs.ArgumentName == _parameterDeclaration.DeclaredName);
        }

        private bool ArgumentIsDefaultedToNull()
        {
            var defaultValue = _parameterDeclaration.DeclaredElement.GetDefaultValue();

            // TODO: not sure that GetDefaultValue could return null!
            return _parameterDeclaration.DeclaredElement.IsOptional
                   && (defaultValue == null
                       || defaultValue.ConstantValue == null
                       || defaultValue.ConstantValue.Value == null);
        }

        private bool MethodIsAbstract()
        {
            // TODO: maybe would be useful to extract this as a state!
            return _functionDeclaration.IsAbstract;
        }

        private bool IsNullableValueType()
        {
            return _parameterDeclaration.Type.IsNullable();
        }

        private bool ParameterIsValueType()
        {
            var declaredElement = _parameterDeclaration.DeclaredElement;
            return declaredElement.Type.IsValueType() && !declaredElement.Type.IsNullable();
        }

    }
}