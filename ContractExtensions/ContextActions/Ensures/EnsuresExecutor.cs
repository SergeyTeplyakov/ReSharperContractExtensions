using JetBrains.ReSharper.Psi.Util;
using ReSharper.ContractExtensions.ContextActions.Ensures;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContractsEx;
using ReSharper.ContractExtensions.ContractUtils;

namespace ReSharper.ContractExtensions.ContextActions.Ensures
{
    internal sealed class EnsuresExecutor : ContextActionExecutorBase
    {
        enum EnsuresType
        {
            NullCheck,
            EnumValidityCheck,
        }
        private readonly ICSharpFunctionDeclaration _selectedFunction;
        private readonly Func<ICSharpFunctionDeclaration, ICSharpStatement> _ensuresFactoryMethod;

        private EnsuresExecutor(ICSharpContextActionDataProvider provider, ICSharpFunctionDeclaration selectedFunction,
            EnsuresType ensuresType)
            : base(provider)
        {
            Contract.Requires(provider != null);
            Contract.Requires(selectedFunction != null);
            Contract.Requires(Enum.IsDefined(typeof (EnsuresType), ensuresType));

            _selectedFunction = selectedFunction;
            _ensuresFactoryMethod = CreateEnsuresFactoryMethod(ensuresType);
        }

        private Func<ICSharpFunctionDeclaration, ICSharpStatement> CreateEnsuresFactoryMethod(EnsuresType ensuresType)
        {
            if (ensuresType == EnsuresType.NullCheck)
                return CreateResultNotNullEnsures;

            return CreateEnumResultIsDefinedEnsures;
        }

        public static EnsuresExecutor CreateNotNullEnsuresExecutor(ICSharpContextActionDataProvider provider,
            ICSharpFunctionDeclaration selectedFunction)
        {
            return new EnsuresExecutor(provider, selectedFunction, EnsuresType.NullCheck);
        }

        public static EnsuresExecutor CreateEnumIsValidEnsuresExecutor(ICSharpContextActionDataProvider provider,
            ICSharpFunctionDeclaration selectedFunction)
        {
            return new EnsuresExecutor(provider, selectedFunction, EnsuresType.EnumValidityCheck);
        }

        protected override void DoExecuteTransaction()
        {
            var functionDeclaration = _selectedFunction;

            // TODO: inconsistency detected! In some cases Availability provides contract function
            // but in some cases executors are used for this.
            var contractFunction = functionDeclaration.GetContractFunction();
            Contract.Assert(contractFunction != null);

            var ensureStatement = _ensuresFactoryMethod(contractFunction);

            var requires = GetLastRequiresStatementIfAny(contractFunction);
            AddStatementAfter(contractFunction, ensureStatement, requires);
        }

        private ICSharpStatement CreateEnumResultIsDefinedEnsures(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);

            string format = "$0.Ensures($1.IsDefined(typeof($2), Contract.Result<$3>()));";
            var returnType = (IDeclaredType)functionDeclaration.DeclaredElement.ReturnType;

            // "typeof" type should be different from the Contract.Result<T> for nullable types!
            var typeofType = returnType.IsNullable() ? returnType.GetNullableUnderlyingType() : returnType;

            var systemEnumType = CreateDeclaredType(typeof(Enum));
            return _factory.CreateStatement(format, ContractType, 
                systemEnumType, typeofType, returnType);
        }

        private ICSharpStatement CreateResultNotNullEnsures(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);

            // TODO: IsValid returns true even if resulting expression would not compiles!
            // To check this, please remove semicolon in the string below.
            // Maybe Resolve method would be good!
            // Contract.Ensures(Contract.Result<ICSharpStatement>().IsValid());

            string format = "$0.Ensures(Contract.Result<$1>() != null);";
            var returnType = (IDeclaredType)functionDeclaration.DeclaredElement.ReturnType;

            return _factory.CreateStatement(format, ContractType, returnType);
        }

        private ICSharpStatement CreateContractEnsures(CSharpElementFactory factory, ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(factory != null);
            Contract.Requires(functionDeclaration != null);
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);
            
            // TODO: IsValid returns true even if resulting expression would not compiles!
            // To check this, please remove semicolon in the string below.
            // Maybe Resolve method would be good!
            // Contract.Ensures(Contract.Result<ICSharpStatement>().IsValid());

            string format = "$0.Ensures(Contract.Result<$1>() != null);";
            var returnType = (IDeclaredType)functionDeclaration.DeclaredElement.ReturnType;

            return factory.CreateStatement(format, ContractType, returnType);
        }

        private ICSharpStatement GetLastRequiresStatementIfAny(ICSharpFunctionDeclaration functionDeclaration)
        {
            return functionDeclaration.GetContractPreconditions()
                .Select(r => r.Statement).LastOrDefault();
        }

        private void AddStatementAfter(ICSharpFunctionDeclaration functionDeclaration,
            ICSharpStatement statement, ICSharpStatement anchor)
        {
            functionDeclaration.Body.AddStatementAfter(statement, anchor);
        }
    }

    
}