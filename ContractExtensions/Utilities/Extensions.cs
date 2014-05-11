using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class Extensions
    {
        public static bool ContainsUsing(this TreeNodeCollection<IUsingDirective> imports, IUsingDirective directive)
        {
            Contract.Requires(directive != null);

            return imports.Any(ud => ud.ImportedSymbolName.QualifiedName == directive.ImportedSymbolName.QualifiedName);
        }

        public static ICSharpStatement CreateStatement(this CSharpElementFactory factory, string statement)
        {
            Contract.Requires(factory != null);
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);

            return factory.CreateStatement(statement, EmptyArray<object>.Instance);
        }
    }
}