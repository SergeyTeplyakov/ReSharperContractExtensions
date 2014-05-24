using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractUtils;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    /// <summary>
    /// Shows whether "Add Requires" action is available or not.
    /// </summary>
    public sealed class RequiresAvailability
    {
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly string _parameterName;
        private readonly ICSharpFunctionDeclaration _functionToInsertPrecondition;

        public readonly static RequiresAvailability Unavailable = new RequiresAvailability {IsAvailable = false};

        private RequiresAvailability()
        {}

        public RequiresAvailability(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            _provider = provider;

            if (ParameterSupportRequires(out _parameterName) &&
                FunctionSupportRequiers(_parameterName, out _functionToInsertPrecondition))
            {
                IsAvailable = true;
            }
        }

        [Pure]
        private bool ParameterSupportRequires(out string parameterName)
        {
            var parameterDeclaration = ParameterRequiresAvailability.Create(_provider);
            if (!parameterDeclaration.IsAvailable)
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
            functionDeclaration = null;

            var selectedFunction = GetSelectedFunctionDeclaration();

            var functionToInsertPrecondition = selectedFunction.GetContractFunction();

            if (!IsFunctionWellDefined(functionToInsertPrecondition))
                return false;

            if (ArgumentIsAlreadyVerifiedByArgCheckOrRequires(functionToInsertPrecondition, parameterName))
                return false;

            functionDeclaration = functionToInsertPrecondition;
            return true;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || _parameterName != null);
            Contract.Invariant(!IsAvailable || _functionToInsertPrecondition != null);
        }

        public static RequiresAvailability Create(ICSharpContextActionDataProvider provider)
        {
            return new RequiresAvailability(provider);
        }

        public bool IsAvailable { get; private set; }
        public ICSharpFunctionDeclaration FunctionToInsertPrecondition { get { return _functionToInsertPrecondition; } }
        public string SelectedParameterName { get { return _parameterName; } }

        [Pure]
        private ICSharpFunctionDeclaration GetSelectedFunctionDeclaration()
        {
            return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
        }

        [Pure]
        private bool IsFunctionWellDefined(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration != null 
                && functionDeclaration.Body != null 
                && functionDeclaration.DeclaredElement != null;
        }

        [Pure]
        private bool ArgumentIsAlreadyVerifiedByArgCheckOrRequires(
            ICSharpFunctionDeclaration functionDeclaration, string parameterName)
        {
            var requiresStatements = functionDeclaration.GetRequires().ToList();

            return requiresStatements.Any(rs => rs.ArgumentName == parameterName);
        }



    }
}