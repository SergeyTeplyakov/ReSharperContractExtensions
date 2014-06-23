using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Shows whether "Add Requires" action is available or not.
    /// </summary>
    public sealed class ArgumentRequiresAvailability : ContextActionAvailabilityBase<ArgumentRequiresAvailability>
    {
        private readonly string _parameterName;
        private readonly IClrTypeName _parameterType;
        private readonly ICSharpFunctionDeclaration _functionToInsertPrecondition;

        public ArgumentRequiresAvailability()
        {}

        public ArgumentRequiresAvailability(ICSharpContextActionDataProvider provider)
            : base(provider)
        {
            Contract.Requires(provider != null);

            if (MethodSupportsRequires(out _parameterName, out _parameterType, out _functionToInsertPrecondition)
                || PropertySetterSupportRequires(out _parameterName, out _parameterType, out _functionToInsertPrecondition))
            {
                _isAvailable = true;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!_isAvailable || _provider != null);
            Contract.Invariant(!_isAvailable || _parameterName != null);
            Contract.Invariant(!_isAvailable || _functionToInsertPrecondition != null);
            Contract.Invariant(!_isAvailable || _parameterType != null);
        }

        protected override void CheckAvailability()
        {}

        private bool MethodSupportsRequires(out string parameterName, out IClrTypeName parameterType,
            out ICSharpFunctionDeclaration functionToInsertPrecondition)
        {
            functionToInsertPrecondition = null;

            return ParameterSupportRequires(out parameterName, out parameterType) &&
                   FunctionSupportRequiers(_parameterName, out functionToInsertPrecondition);
        }

        [Pure]
        private bool ParameterSupportRequires(out string parameterName, out IClrTypeName parameterType)
        {
            var parameterDeclaration = ParameterRequiresAvailability.Create(_provider);
            if (parameterDeclaration.IsAvailable)
            {
                parameterName = parameterDeclaration.ParameterName;
                parameterType = parameterDeclaration.ParameterType;
                return true;
            }

            parameterName = null;
            parameterType = null;
            return false;
        }

        [Pure]
        private bool FunctionSupportRequiers(string parameterName, out ICSharpFunctionDeclaration functionDeclaration)
        {
            var func = new FunctionRequiresAvailability(_provider, parameterName);
            if (func.IsAvailable)
            {
                functionDeclaration = func.FunctionToInsertPrecondition;
                return true;
            }

            functionDeclaration = null;
            return false;
        }

        private bool PropertySetterSupportRequires(out string parameterName, out IClrTypeName parameterType,
            out ICSharpFunctionDeclaration functionToInsertPrecondition)
        {
            parameterName = null;
            parameterType = null;
            functionToInsertPrecondition = null;

            var propertySetterAvailability = new PropertySetterRequiresAvailability(_provider);
            if (!propertySetterAvailability.IsAvailable)
                return false;

            parameterName = "value";
            parameterType = propertySetterAvailability.PropertyType;

            var func = new FunctionRequiresAvailability(_provider, parameterName,
                propertySetterAvailability.SelectedFunctionDeclaration);

            if (func.IsAvailable)
            {
                functionToInsertPrecondition = func.FunctionToInsertPrecondition;
                return true;
            }

            return false;
        }

        public ICSharpFunctionDeclaration FunctionToInsertPrecondition { get { return _functionToInsertPrecondition; } }
        public string SelectedParameterName { get { return _parameterName; } }
        public IClrTypeName SelectedParameterType { get { return _parameterType; } }
    }
}