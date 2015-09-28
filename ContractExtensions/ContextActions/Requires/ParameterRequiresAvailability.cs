using System.Diagnostics.Contracts;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    
    /// <summary>
    /// Checkes that Contract.Requires should be available based on parameter declaration.
    /// </summary>
    /// <remarks>
    /// Extracting this class out from the main <see cref="AddRequiresAvailability"/> explained by
    /// the need of this check out of the main requires availability check.
    /// For example, this stuff is needed by <see cref="ComboRequiresAvailability"/> class as well.
    /// </remarks>
    internal class ParameterRequiresAvailability : ContextActionAvailabilityBase<ParameterRequiresAvailability>
    {
        private readonly IParameterDeclaration _parameterDeclaration;

        public ParameterRequiresAvailability()
        {}

        private ParameterRequiresAvailability(ICSharpContextActionDataProvider provider, 
            IParameterDeclaration parameterDeclaration)
            : base(provider)
        {
            Contract.Requires(parameterDeclaration != null);

            _parameterDeclaration = parameterDeclaration;
            _isAvailable = ComputeIsAvailable();

            if (_isAvailable)
            {
                ParameterName = _parameterDeclaration.DeclaredName;

                ParameterType = _parameterDeclaration.Type.GetClrTypeName();
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _parameterDeclaration != null);
            Contract.Invariant(!IsAvailable || ParameterName != null);
            Contract.Invariant(!IsAvailable || ParameterType != null);
        }

        public static ParameterRequiresAvailability Create(ICSharpContextActionDataProvider provider,
            IParameterDeclaration selectedParameter = null)
        {
            Contract.Requires(provider != null);

            selectedParameter = selectedParameter ?? provider.GetSelectedParameterDeclaration();
            if (selectedParameter == null)
                return Unavailable;

            return new ParameterRequiresAvailability(provider, selectedParameter);
        }

        public string ParameterName { get; private set; }
        public IClrTypeName ParameterType { get; private set; }

        private bool ComputeIsAvailable()
        {
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
            return _parameterDeclaration.IsDefaultedToNull();
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