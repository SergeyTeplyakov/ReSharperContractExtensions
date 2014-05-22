using System.Collections.Generic;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Feature.Services.Generate.Actions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using JetBrains.Util;
using DataConstants = JetBrains.ProjectModel.DataContext.DataConstants;

namespace ActiveMesa.R2P.Generators
{
    [ActionHandler("Generate.ReadOnlyInterface")]
    public class ReadOnlyInterfaceAction : GenerateActionBase<ReadOnlyInterfaceItemProvider>
    {
        protected override bool ShowMenuWithOneItem
        {
            get { return true; }
        }

        protected override RichText Caption
        {
            get { return "Generate Read-Only Interface"; }
        }
    }

    [GenerateProvider]
    public class ReadOnlyInterfaceItemProvider : IGenerateActionProvider
    {
        public IEnumerable<IGenerateActionWorkflow> CreateWorkflow(IDataContext dataContext)
        {
            var solution = dataContext.GetData(DataConstants.SOLUTION);
            var iconManager = solution.GetComponent<PsiIconManager>();
            var icon = iconManager.GetImage(CLRDeclaredElementType.INTERFACE);
            yield return new ReadOnlyInterfaceActionWorkflow(icon);
        }
    }

    public class ReadOnlyInterfaceActionWorkflow : StandardGenerateActionWorkflow
    {
        public ReadOnlyInterfaceActionWorkflow(IconId icon)
          : base("ReadOnlyInterface", icon, "Read-only interface", GenerateActionGroup.CLR_LANGUAGE, "Generate read-only interface",
                 "Generates a read-only interface for this class.", "Generate.ReadOnlyInterface")
        {
        }

        public override double Order
        {
            get { return 1; }
        }

        /// <summary>
        /// This method is redefined in order to get rid of the IsKindAllowed() check at the end.
        /// </summary>
        public override bool IsAvailable(IDataContext dataContext)
        {
            var solution = dataContext.GetData(DataConstants.SOLUTION);
            if (solution == null)
                return false;

            var generatorManager = GeneratorManager.GetInstance(solution);
            if (generatorManager == null)
                return false;

            var languageType = generatorManager.GetPsiLanguageFromContext(dataContext);
            if (languageType == null)
                return false;

            var generatorContextFactory = LanguageManager.Instance.TryGetService<IGeneratorContextFactory>(languageType);
            return generatorContextFactory != null;
        }
    }

    [GeneratorBuilder("ReadOnlyInterface", typeof(CSharpLanguage))]
    public class ReadOnlyInterfaceBuilder : GeneratorBuilderBase<CSharpGeneratorContext>
    {
        protected override bool IsAvaliable(CSharpGeneratorContext context)
        {
            return base.IsAvaliable(context);
            // fuckup: R# thinks all classes are static

            // class cannot be static or abstract
            //return context.ClassDeclaration != null &&
            //       !context.ClassDeclaration.IsAbstract &&
            //       !context.ClassDeclaration.IsStatic;
        }

        protected override void Process(CSharpGeneratorContext context)
        {
            var factory = CSharpElementFactory.GetInstance(context.PsiModule);
            ReadOnlyIntefaceBuilderWorkflow.Start(context, factory);
        }

        public override double Priority
        {
            get { return 0; }
        }

        private static class ReadOnlyIntefaceBuilderWorkflow
        {
            public static WorkflowResult Start([CanBeNull] CSharpGeneratorContext context, CSharpElementFactory factory)
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
    }

    internal class WorkflowResult
    {
        public static WorkflowResult Inapplicable { get; set; }
        public static WorkflowResult Success { get; set; }
    }

    [GeneratorElementProvider("ReadOnlyInterface", typeof(CSharpLanguage))]
    internal class ReadOnlyInterfacePropertyProvider : GeneratorProviderBase<CSharpGeneratorContext>
    {
        /// <summary>
        /// If we have several providers for the same generate kind, this property will set order on them
        /// </summary>
        public override double Priority
        {
            get { return 0; }
        }

        /// <summary>
        /// Populate context with input elements
        /// </summary>
        /// <param name="context"></param>
        public override void Populate(CSharpGeneratorContext context)
        {
            var typeElement = context.ClassDeclaration.DeclaredElement;
            if (typeElement == null)
                return;

            if (!(typeElement is IStruct) && !(typeElement is IClass))
                return;
            // provide only readable properies
            var stuff =
              context.ClassDeclaration.PropertyDeclarations.Select(
                member => new { member, memberType = member.Type as IDeclaredType }).Where(
                  t => !t.member.IsStatic &&
                       !t.member.IsAbstract &&
                       t.member.DeclaredElement.IsReadable && // must be readable
                       !t.member.IsSynthetic() &&
                       t.memberType != null &&
                       t.memberType.CanUseExplicitly(context.ClassDeclaration))
                .Select(u => new GeneratorDeclaredElement<ITypeOwner>(u.member.DeclaredElement));
            context.ProvidedElements.AddRange(stuff);
        }
    }

}