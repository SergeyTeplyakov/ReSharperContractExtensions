using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
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

        public static void AddUsingForTypeIfNecessary(this ICSharpFile currentFile, System.Type type)
        {
            Contract.Requires(currentFile != null);
            Contract.Requires(type != null);

            AddUsingForNamespaceIfNecessary(currentFile, type.Namespace);
        }

        public static void AddUsingForTypeIfNecessary(this ICSharpFile currentFile, IClrTypeName typeName)
        {
            Contract.Requires(currentFile != null);
            Contract.Requires(typeName != null);

            var @namespace = string.Join(".", typeName.NamespaceNames);

            if (!string.IsNullOrEmpty(@namespace))
                AddUsingForNamespaceIfNecessary(currentFile, @namespace);
        }

        private static void AddUsingForNamespaceIfNecessary(this ICSharpFile currentFile, string @namespace)
        {
            Contract.Requires(!string.IsNullOrEmpty(@namespace));

            var factory = CSharpElementFactory.GetInstance(currentFile);

            Contract.Assert(factory != null);

            var usingDirective = factory.CreateUsingDirective("using $0", @namespace);
            if (!currentFile.Imports.ContainsUsing(usingDirective))
            {
                UsingUtil.AddImportAfter(currentFile, usingDirective);
            }
        }


        public static ICSharpStatement CreateStatement(this CSharpElementFactory factory, string statement)
        {
            Contract.Requires(factory != null);
            Contract.Ensures(Contract.Result<ICSharpStatement>() != null);

            return factory.CreateStatement(statement, EmptyArray<object>.Instance);
        }
    }
}