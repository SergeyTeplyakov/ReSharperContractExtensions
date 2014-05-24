using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Checkes that Contract.Requires should be available based on parameter declaration.
    /// </summary>
    /// <remarks>
    /// Extracting this class out from the main <see cref="RequiresAvailability"/> explained by
    /// the need of this check out of the main requires availability check.
    /// For example, this stuff is needed by <see cref="ComboRequiresAvailability"/> class as well.
    /// </remarks>
    internal sealed class ParameterRequiresAvailability
    {
        private readonly IParameterDeclaration _parameterDeclaration;

        private ParameterRequiresAvailability()
        {
            IsAvailable = false;
        }

        private ParameterRequiresAvailability(IParameterDeclaration parameterDeclaration)
        {
            Contract.Requires(parameterDeclaration != null);

            _parameterDeclaration = parameterDeclaration;
            IsAvailable = ComputeIsAvailable();

            if (IsAvailable)
            {
                ParameterName = _parameterDeclaration.DeclaredName;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _parameterDeclaration != null);
            Contract.Invariant(!IsAvailable || ParameterName != null);
        }

        public static ParameterRequiresAvailability Create(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            var selectedParameter = provider.GetSelectedParameterDeclaration();
            if (selectedParameter == null)
                return new ParameterRequiresAvailability {IsAvailable = false};

            return new ParameterRequiresAvailability(selectedParameter);
        }


        public bool IsAvailable { get; private set; }
        public string ParameterName { get; private set; }

        private bool ComputeIsAvailable()
        {
            Contract.Assert(_parameterDeclaration != null);

            if (!IsParameterDeclarationWellDefined())
                return false;

            if (IsParameterOutput())
                return false;

            if (IsParameterDefaultedToNull())
                return false;

            if (IsParameterValueType() && !IsParameterNullableValueType())
                return false;

            return true;
        }

        [Pure]
        private bool IsParameterDeclarationWellDefined()
        {
            return _parameterDeclaration != null && _parameterDeclaration.DeclaredElement != null;
        }

        [Pure]
        private bool IsParameterOutput()
        {
            return _parameterDeclaration.DeclaredElement.Kind == ParameterKind.OUTPUT;
        }

        [Pure]
        private bool IsParameterDefaultedToNull()
        {
            var defaultValue = _parameterDeclaration.DeclaredElement.GetDefaultValue();

            // TODO: not sure that GetDefaultValue could return null!
            return _parameterDeclaration.DeclaredElement.IsOptional
                   && (defaultValue == null
                       || defaultValue.ConstantValue == null
                       || defaultValue.ConstantValue.Value == null);
        }

        [Pure]
        private bool IsParameterNullableValueType()
        {
            return _parameterDeclaration.Type.IsNullable();
        }

        [Pure]
        private bool IsParameterValueType()
        {
            var declaredElement = _parameterDeclaration.DeclaredElement;
            return declaredElement.Type.IsValueType() && !declaredElement.Type.IsNullable();
        }
    }
}