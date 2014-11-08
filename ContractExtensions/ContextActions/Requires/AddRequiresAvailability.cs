using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
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
    // TODO: rename to null-check requiers availability
    public sealed class AddRequiresAvailability : ContextActionAvailabilityBase<AddRequiresAvailability>
    {
        private readonly string _parameterName;
        private readonly IClrTypeName _parameterType;
        private readonly ICSharpFunctionDeclaration _functionToInsertPrecondition;
        private readonly ReadOnlyCollection<ICSharpFunctionDeclaration> _functionsToInsertPrecondition;

        public AddRequiresAvailability()
        {}

        public AddRequiresAvailability(ICSharpContextActionDataProvider provider)
            : base(provider)
        {
            Contract.Requires(provider != null);

            if (MethodSupportsRequires(out _parameterName, out _parameterType, out _functionsToInsertPrecondition)
                || PropertySetterSupportRequires(out _parameterName, out _parameterType, out _functionsToInsertPrecondition))
            {
                _isAvailable = true;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!_isAvailable || _provider != null);
            Contract.Invariant(!_isAvailable || _parameterName != null);
            Contract.Invariant(!_isAvailable || FunctionToInsertPrecondition != null);
            Contract.Invariant(!_isAvailable || _parameterType != null);
        }

        protected override void CheckAvailability()
        {}

        private bool MethodSupportsRequires(out string parameterName, out IClrTypeName parameterType,
            out ReadOnlyCollection<ICSharpFunctionDeclaration> functionsToInsertPrecondition)
        {
            functionsToInsertPrecondition = null;

            return ParameterSupportRequires(out parameterName, out parameterType) &&
                   FunctionSupportRequiers(parameterName, out functionsToInsertPrecondition);
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
        private bool FunctionSupportRequiers(string parameterName, out ReadOnlyCollection<ICSharpFunctionDeclaration> functionsDeclaration)
        {
            var func = new FunctionRequiresAvailability(_provider, parameterName);
            if (func.IsAvailable)
            {
                functionsDeclaration = func.FunctionsToInsertPrecondition;
                return true;
            }

            functionsDeclaration = null;
            return false;
        }

        private bool PropertySetterSupportRequires(out string parameterName, out IClrTypeName parameterType,
            out ReadOnlyCollection<ICSharpFunctionDeclaration> functionsToInsertPrecondition)
        {
            parameterName = null;
            parameterType = null;
            functionsToInsertPrecondition = null;

            var propertySetterAvailability = new PropertySetterRequiresAvailability(_provider);
            if (!propertySetterAvailability.IsAvailable)
                return false;

            parameterName = propertySetterAvailability.ParameterName;
            parameterType = propertySetterAvailability.PropertyType;

            var func = new FunctionRequiresAvailability(_provider, parameterName,
                propertySetterAvailability.GetSelectedFunctions());

            if (func.IsAvailable)
            {
                functionsToInsertPrecondition = func.FunctionsToInsertPrecondition;
                return true;
            }

            return false;
        }

        public ReadOnlyCollection<ICSharpFunctionDeclaration> FunctionsToInsertPrecondition
        {
            get { return _functionsToInsertPrecondition ?? new List<ICSharpFunctionDeclaration>().AsReadOnly(); }
        }

        public ICSharpFunctionDeclaration FunctionToInsertPrecondition { get { return FunctionsToInsertPrecondition.FirstOrDefault(); } }
        public string SelectedParameterName { get { return _parameterName; } }
        public IClrTypeName SelectedParameterType { get { return _parameterType; } }
    }
}