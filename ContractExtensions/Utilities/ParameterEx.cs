using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.Utilities
{
    public static class ParameterEx
    {
        public static bool IsDefaultedToNull(this IParameter parameter)
        {
            Contract.Requires(parameter != null);

            var defaultValue = parameter.GetDefaultValue();

            // TODO: not sure that GetDefaultValue could return null!
            return parameter.IsOptional
                   && (defaultValue.With(x => x.ConstantValue).With(x => x.Value) == null);
        }

        public static bool IsDefaultedToNull(this IParameterDeclaration parameterDeclaration)
        {
            Contract.Requires(parameterDeclaration != null);

            Contract.Assert(parameterDeclaration.DeclaredElement != null);

            var defaultValue = parameterDeclaration.DeclaredElement.GetDefaultValue();

            // TODO: not sure that GetDefaultValue could return null!
            return parameterDeclaration.DeclaredElement.IsOptional
                   && (defaultValue.With(x => x.ConstantValue).With(x => x.Value) == null);
        }

        public static bool HasClrAttribute(this IParameterDeclaration parameterDeclaration, System.Type attributeType)
        {
            Contract.Requires(parameterDeclaration != null);
            Contract.Requires(attributeType != null);
            Contract.Requires(attributeType.IsAttribute());

            Contract.Assert(parameterDeclaration.DeclaredElement != null);

            return parameterDeclaration.DeclaredElement.HasAttributeInstance(
                new ClrTypeName(attributeType.FullName), false);
        }

        public static bool HasClrAttribute(this IParameter parameter, System.Type attributeType)
        {
            Contract.Requires(parameter != null);
            Contract.Requires(attributeType != null);
            Contract.Requires(attributeType.IsAttribute());
            
            // TODO: in tests "JetBrains.Annotation.CanBeNull" could not be resolved, that's why
            // attributeInstance.GetClrName returns unresolved name without apropriate full name.
            // So, I've added a workaround and trying to compare with "PresentableName" which is "CanBeNull" in this case.
            return parameter
                .GetAttributeInstances(false)
                .Any(a => a.GetClrName().With(x => x.FullName) == attributeType.FullName || 
                     attributeType.Name.Contains(a.GetAttributeType().GetPresentableName(CSharpLanguage.Instance)));
        }
    }
}