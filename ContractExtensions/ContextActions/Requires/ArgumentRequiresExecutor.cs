using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.Settings;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    internal sealed class ArgumentRequiresExecutor : ContextActionExecutorBase
    {
        private readonly string _parameterName;
        private readonly IClrTypeName _propertyType;

        private readonly bool _shouldBeGeneric;

        private readonly ICSharpFunctionDeclaration _functionDeclaration;

        public ArgumentRequiresExecutor(ICSharpContextActionDataProvider provider, bool shouldBeGeneric, 
            ICSharpFunctionDeclaration functionDeclaration, string parameterName, IClrTypeName propertyType)
            : base(provider)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Requires(parameterName != null);
            Contract.Requires(propertyType != null);

            _shouldBeGeneric = shouldBeGeneric;
            _functionDeclaration = functionDeclaration;
            _parameterName = parameterName;
            _propertyType = propertyType;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_parameterName != null);
            Contract.Invariant(_propertyType != null);
            Contract.Invariant(_functionDeclaration != null);
        }

        public override void ExecuteTransaction()
        {
            var statement = CreateContractRequires();
            var addAfter = GetPreviousRequires();

            // Tried to fix an issue that I can't find a precondition for the newly created Contract.Requires
            // Controlflow talked about physical trees, but in this case didn't help!!
            //var declaration = (ICSharpFunctionDeclaration)CSharpFunctionDeclarationNavigator.GetByBody(_functionDeclaration.Body);
            //declaration.Body.AddStatementAfter(statement, addAfter);

            _functionDeclaration.Body.AddStatementAfter(statement, addAfter);
        }

        [System.Diagnostics.Contracts.Pure]
        private ICSharpStatement CreateContractRequires()
        {
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);

            var compareExpression = CreateCompareExpression();
            IDeclaredType contract = CreateDeclaredType(typeof(Contract));

            if (_shouldBeGeneric)
            {
                IDeclaredType argumentNullException = GetPredefinedType().ArgumentNullException;
                string format = string.Format("$0.Requires<$1>($2);");
                return _factory.CreateStatement(format, contract, argumentNullException, compareExpression);
            }
            else
            {
                string format = string.Format("$0.Requires($1);");
                return _factory.CreateStatement(format, contract, compareExpression);
            }
        }

        private ICSharpExpression CreateCompareExpression()
        {
            if (_propertyType.FullName == typeof (string).FullName
                && ShouldCheckStringsForNullOrEmpty())
            {

                return _factory.CreateExpression("!string.IsNullOrEmpty($0)", _parameterName);
            }
            return _factory.CreateExpression("$0 != null", _parameterName);
        }

        private bool ShouldCheckStringsForNullOrEmpty()
        {
            var settings = _provider.SourceFile.GetSettingsStore()
                .GetKey<ContractExtensionsSettings>(SettingsOptimization.OptimizeDefault);

            return settings.CheckStringsForNullOrEmpty;
        }

        [System.Diagnostics.Contracts.Pure, CanBeNull]
        ICSharpStatement GetPreviousRequires()
        {
            return _functionDeclaration.GetLastPreconditionFor(_parameterName).With(x => x.Statement);
        }
    }
}