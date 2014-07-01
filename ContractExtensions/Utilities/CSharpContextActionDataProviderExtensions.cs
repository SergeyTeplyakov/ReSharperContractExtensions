using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.Utilities
{
    public static class CSharpContextActionDataProviderExtensions
    {
        [Pure]
        public static IParameterDeclaration GetSelectedParameterDeclaration(
            this ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);

            // If the carret is on the parameter _parameterDeclaration,
            // GetSelectedElement will return not-null value
            var parameterDeclaration = provider.GetSelectedElement<IParameterDeclaration>(true, true);
            if (parameterDeclaration != null)
                return parameterDeclaration;

            // But parameter could be selected inside method body
            var selectedDeclaration = provider.GetSelectedElement<IReferenceExpression>(true, true)
                .With(x => x.Reference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement)
                .With(x => x.GetDeclarations().FirstOrDefault())
                .Return(x => x as IParameterDeclaration);

            if (selectedDeclaration == null)
                return null;

            var currentFunction =
                provider.GetSelectedElement<IFunctionDeclaration>(true, true);

            if (currentFunction == null)
                return null;

            bool isArgument =
                currentFunction.DeclaredElement.Parameters
                .SelectMany(pm => pm.GetDeclarations())
                .Select(pm => pm as IParameterDeclaration)
                .Where(pm => pm != null)
                .Contains(selectedDeclaration);

            return isArgument ? selectedDeclaration : null;
        }

        public static bool IsValidForContractContextActions(this ICSharpContextActionDataProvider provider)
        {
            return provider != null && provider.SelectedElement != null;
        }
    }
}