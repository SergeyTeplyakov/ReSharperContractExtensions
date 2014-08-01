using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.Utilities
{
    public static class ExpressionEx
    {
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

        public static IEnumerable<TExpression> ProcessRecursively<TExpression>(this IExpression expression)
            where TExpression : class, IExpression
            
        {
            var processor = new Procesor<TExpression>();
            expression.ProcessThisAndDescendants(processor);
            return processor.ProcessedNodes;
        }

        private class Procesor<T> : IRecursiveElementProcessor where T : class, IExpression
        {
            public Procesor()
            {
                ProcessedNodes = new List<T>();
            }

            public List<T> ProcessedNodes { get; private set; }
            public bool InteriorShouldBeProcessed(ITreeNode element)
            {
                //if (element is T)
                //    return false;

                return true;
            }

            public void ProcessBeforeInterior(ITreeNode element)
            {
                var t = element as T;
                if (t != null)
                    ProcessedNodes.Add(t);
            }

            public void ProcessAfterInterior(ITreeNode element)
            {

            }

            public bool ProcessingIsFinished { get; private set; }
        }
    }
}