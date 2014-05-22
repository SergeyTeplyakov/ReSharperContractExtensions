using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi.CSharp;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor.Generator
{
    [GeneratorBuilder("ContractClass", typeof (CSharpLanguage))]
    public class ContractClassBuilder : GeneratorBuilderBase<CSharpGeneratorContext>
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
            var contractClassGenerator = new ContractClassGenerator(context, factory);
            //ContractClassBuilder.ReadOnlyIntefaceBuilderWorkflow.Start(context, factory);
        }

        public override double Priority
        {
            get { return 0; }
        }
    }
}