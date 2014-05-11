using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.Special;

namespace ReSharper.ContractExtensions.Utilities
{
    public static class ExpressionEx
    {
        public static bool IsSelected<T>(this ICSharpContextActionDataProvider provider) where T : class, ITreeNode
        {
            Contract.Requires(provider != null);

            return provider.GetSelectedElement<T>(true, true) != null;
        }

        [CanBeNull]
        public static IClrTypeName GetCallSiteType(this IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            var type = invocationExpression
                .With(x => x.InvokedExpression)
                .With(x => x as IReferenceExpression)
                .With(x => x.Reference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement)
                .With(x => x as IClrDeclaredElement)
                .With(x => x.GetContainingType())
                .Return(x => x.GetClrName());
            return type;
        }

        [CanBeNull]
        public static string GetCalledMethod(this IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            var result = invocationExpression
                .With(x => x.InvokedExpression)
                .With(x => x as IReferenceExpression)
                .With(x => x.Reference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement)
                .With(x => x as IClrDeclaredElement)
                .Return(x => x.ShortName);
            return result;
        }

        [CanBeNull]
        public static IClrDeclaredElement GetClrDeclaredElement(this IInvocationExpression invocationExpression)
        {
            Contract.Requires(invocationExpression != null);

            var result = invocationExpression
                .With(x => x.InvokedExpression)
                .With(x => x as IReferenceExpression)
                .With(x => x.Reference)
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement)
                .With(x => x as IClrDeclaredElement);
            return result;
        }
    }

    
}