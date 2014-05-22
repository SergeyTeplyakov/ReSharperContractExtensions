using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor.Generator
{
    internal sealed class ContractClassGenerator
    {
        private readonly CSharpElementFactory _factory;
        private readonly CSharpGeneratorContext _context;

        public ContractClassGenerator(CSharpGeneratorContext context, CSharpElementFactory factory)
        {
            Contract.Requires(context != null);
            Contract.Requires(factory != null);

            _factory = factory;
            _context = context;
        }

        public void Process()
        { }
        private static class ReadOnlyIntefaceBuilderWorkflow
        {
            public static WorkflowResult Start(CSharpGeneratorContext context, CSharpElementFactory factory)
            {
                if (context.ClassDeclaration == null) return WorkflowResult.Inapplicable;
                return CreateNewInterfaceDeclaration(context, factory);
            }

            private static WorkflowResult CreateNewInterfaceDeclaration(CSharpGeneratorContext context, CSharpElementFactory factory)
            {
                string interfaceName = "IReadOnly" + context.ClassDeclaration.DeclaredName;
                var itfDecl = (IInterfaceDeclaration)factory.CreateTypeMemberDeclaration("interface " + interfaceName + " {}");
                return AddInterfaceToContainingNamespace(context, itfDecl, factory);
            }

            private static WorkflowResult AddInterfaceToContainingNamespace(CSharpGeneratorContext context, IInterfaceDeclaration itfDecl, CSharpElementFactory factory)
            {
                var ns = context.ClassDeclaration.GetContainingNamespaceDeclaration();
                if (ns == null) return WorkflowResult.Inapplicable;
                else
                {
                    var typeDecl = ns.AddTypeDeclarationBefore(itfDecl, context.ClassDeclaration);
                    return PopulateInterfaceDeclaration(context, factory, (IInterfaceDeclaration)typeDecl);
                }
            }

            private static WorkflowResult PopulateInterfaceDeclaration(CSharpGeneratorContext context, CSharpElementFactory factory, IInterfaceDeclaration itfDecl)
            {
                var props = context.InputElements.OfType<GeneratorDeclaredElement<ITypeOwner>>().ToList();
                foreach (var prop in props)
                {
                    var propDecl = (IClassMemberDeclaration)factory.CreateTypeMemberDeclaration("public $0 $1 { get; }", prop.DeclaredElement.Type, prop.DeclaredElement.ShortName);
                    itfDecl.AddClassMemberDeclaration(propDecl);
                }
                return EnsureClassImplementsInterface(context, itfDecl);
            }

            private static WorkflowResult EnsureClassImplementsInterface(CSharpGeneratorContext context, IInterfaceDeclaration itfDecl)
            {
                var interfaceType = TypeFactory.CreateType(itfDecl.DeclaredElement);
                context.ClassDeclaration.AddSuperInterface(interfaceType, false);
                return WorkflowResult.Success;
            }
        }

        internal class WorkflowResult
        {
            public static WorkflowResult Inapplicable { get; set; }
            public static WorkflowResult Success { get; set; }
        }
    }
}