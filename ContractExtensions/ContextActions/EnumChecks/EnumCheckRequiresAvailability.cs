using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using ReSharper.ContractExtensions.ContextActions.Infrastructure;
using ReSharper.ContractExtensions.ContextActions.Requires;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.EnumChecks
{
    internal sealed class EnumCheckRequiresAvailability : ContextActionAvailabilityBase<EnumCheckRequiresAvailability>
    {
        private readonly IParameterDeclaration _selectedParameterDeclaration;
        private readonly ICSharpFunctionDeclaration _functionToInsertPrecondition;

        public EnumCheckRequiresAvailability()
        {}

        public EnumCheckRequiresAvailability(ICSharpContextActionDataProvider provider) : base(provider)
        {
            _isAvailable = ComputeAvailability(out _selectedParameterDeclaration,
                out _functionToInsertPrecondition);
        }

        public IParameterDeclaration ParameterDeclaration
        {
            get
            {
                Contract.Ensures(!IsAvailable || Contract.Result<IParameterDeclaration>() != null);
                return _selectedParameterDeclaration;
            }
        }

        public string ParameterName
        {
            get
            {
                Contract.Ensures(!IsAvailable || Contract.Result<string>() != null);
                return _selectedParameterDeclaration.DeclaredName;
            }
        }

        public IClrTypeName ParameterUnderlyingType
        {
            get
            {
                Contract.Ensures(!IsAvailable || Contract.Result<IClrTypeName>() != null);
                var type = _selectedParameterDeclaration.Type;
                return (type.IsNullable() ? type.GetNullableUnderlyingType() : type).GetClrTypeName();
            }
        }

        public IClrTypeName ParameterType
        {
            get
            {
                Contract.Ensures(!IsAvailable || Contract.Result<IClrTypeName>() != null);
                return _selectedParameterDeclaration.Type.GetClrTypeName();
            }
        }

        public ICSharpFunctionDeclaration FunctionToInsertPrecondition
        {
            get
            {
                Contract.Ensures(!IsAvailable || Contract.Result<ICSharpFunctionDeclaration>() != null);
                return _functionToInsertPrecondition;
            }
        }

        private bool ComputeAvailability(out IParameterDeclaration parameterDeclaration,
            out ICSharpFunctionDeclaration functionToInsertPrecondition)
        {
            functionToInsertPrecondition = null;

            parameterDeclaration = _provider.GetSelectedParameterDeclaration();

            if (parameterDeclaration == null)
                return false;

            if (!IsEnum(parameterDeclaration) && !IsNullableEnum(parameterDeclaration))
                return false;

            if (!ContractFunctionIsWellDefinedAndDidntContainPrecondition(parameterDeclaration.DeclaredName, out functionToInsertPrecondition))
                return false;

            return true;
        }

        private bool ContractFunctionIsWellDefinedAndDidntContainPrecondition(string parameterName, 
            out ICSharpFunctionDeclaration functionToInsertPrecondition)
        {
            Contract.Requires(!string.IsNullOrEmpty(parameterName));

            var func = new FunctionRequiresAvailability(_provider, parameterName);
            if (func.IsAvailable)
            {
                functionToInsertPrecondition = func.FunctionToInsertPrecondition;
                return true;
            }

            functionToInsertPrecondition = null;
            return false;
        }

        private bool IsNullableEnum(IParameterDeclaration parameterDeclaration)
        {
            Contract.Requires(parameterDeclaration != null);
            Contract.Assert(parameterDeclaration.Type != null);

            return parameterDeclaration.Type.IsNullable() &&
                   parameterDeclaration.Type.GetNullableUnderlyingType().IsEnumType();
        }


        private bool IsEnum(IParameterDeclaration parameterDeclaration)
        {
            Contract.Requires(parameterDeclaration != null);
            Contract.Assert(parameterDeclaration.Type != null);

            return parameterDeclaration.Type.IsEnumType();
        }

    }
}