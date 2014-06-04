using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Shows whether "Add Requires" action is available or not.
    /// </summary>
    public sealed class ArgumentRequiresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly string _parameterName;
        private readonly ICSharpFunctionDeclaration _functionToInsertPrecondition;

        public readonly static ArgumentRequiresAvailability Unavailable = new ArgumentRequiresAvailability {IsAvailable = false};

        private ArgumentRequiresAvailability()
        {}

        public ArgumentRequiresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            _provider = provider;

            if (MethodSupportsRequires(out _parameterName, out _functionToInsertPrecondition)
                || PropertySetterSupportRequires(out _parameterName, out _functionToInsertPrecondition))
            {
                IsAvailable = true;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _parameterName != null);
            Contract.Invariant(!IsAvailable || _functionToInsertPrecondition != null);
        }

        public static ArgumentRequiresAvailability Create(ICSharpContextActionDataProvider provider)
        {
            return new ArgumentRequiresAvailability(provider);
        }

        private bool MethodSupportsRequires(out string parameterName, out ICSharpFunctionDeclaration functionToInsertPrecondition)
        {
            functionToInsertPrecondition = null;

            return ParameterSupportRequires(out parameterName) &&
                   FunctionSupportRequiers(_parameterName, out functionToInsertPrecondition);
        }

        [Pure]
        private bool ParameterSupportRequires(out string parameterName)
        {
            var parameterDeclaration = ParameterRequiresAvailability.Create(_provider);
            if (parameterDeclaration.IsAvailable)
            {
                parameterName = parameterDeclaration.ParameterName;
                return true;
            }

            parameterName = null;
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

        private bool PropertySetterSupportRequires(out string parameterName, out ICSharpFunctionDeclaration functionToInsertPrecondition)
        {
            parameterName = null;
            functionToInsertPrecondition = null;

            var propertySetterAvailability = new PropertySetterRequiresAvailability(_provider);
            if (!propertySetterAvailability.IsAvailable)
                return false;

            parameterName = "value";
            var func = new FunctionRequiresAvailability(_provider, parameterName,
                propertySetterAvailability.SelectedFunctionDeclaration);

            if (func.IsAvailable)
            {
                functionToInsertPrecondition = func.FunctionToInsertPrecondition;
                return true;
            }

            return false;
        }

        public bool IsAvailable { get; private set; }
        public ICSharpFunctionDeclaration FunctionToInsertPrecondition { get { return _functionToInsertPrecondition; } }
        public string SelectedParameterName { get { return _parameterName; } }
    }
}