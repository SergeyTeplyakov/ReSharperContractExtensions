using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.Types;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.EnumChecks
{
    internal sealed class EnumCheckRequiresExecutor : ContextActionExecutorBase
    {
        private readonly EnumCheckRequiresAvailability _availability;
        private readonly bool _useGenericVersion;
        private readonly ICSharpFunctionDeclaration _functionDeclaration;

        public EnumCheckRequiresExecutor(EnumCheckRequiresAvailability availability, bool useGenericVersion) 
            : base(availability)
        {
            _availability = availability;
            _useGenericVersion = useGenericVersion;
            _functionDeclaration = _availability.FunctionToInsertPrecondition;
        }

        public override void ExecuteTransaction()
        {
            var statement = CreateRequiresStatement(_useGenericVersion);
            var previousRequires = GetPreviousRequires();

            _functionDeclaration.Body.AddStatementAfter(statement, previousRequires);
        }

        [System.Diagnostics.Contracts.Pure]
        private ICSharpStatement CreateRequiresStatement(bool isGeneric)
        {
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);

            IDeclaredType contractType = CreateDeclaredType(typeof(Contract));
            IDeclaredType systemEnumType = CreateDeclaredType(typeof(System.Enum));
            IDeclaredType customEnumType = CreateDeclaredType(_availability.ParameterType);

            ICSharpStatement statement = null;
            if (isGeneric)
            {
                string format = "$0.Requires<$1>($2.IsDefined(typeof($3), $4));";
                IDeclaredType argumentException = GetPredefinedType().ArgumentException;

                statement = _factory.CreateStatement(format,
                    contractType, argumentException, systemEnumType, customEnumType, _availability.ParameterName);
            }
            else
            {
                string format = "$0.Requires($1.IsDefined(typeof($2), $3));";
                statement = _factory.CreateStatement(format,
                    contractType, systemEnumType, customEnumType, _availability.ParameterName);
            }

            return statement;
        }

        [System.Diagnostics.Contracts.Pure, CanBeNull]
        ICSharpStatement GetPreviousRequires()
        {
            return _availability.FunctionToInsertPrecondition.GetLastPreconditionFor(
                _availability.ParameterName).With(x => x.Statement);
        }


    }
}