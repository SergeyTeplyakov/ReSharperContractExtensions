using System;
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Feature.Services.Generate.Actions;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.Icons;
using ReSharper.ContractExtensions.Utilities;
using DataConstants = JetBrains.ProjectModel.DataContext.DataConstants;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor.Generator
{
    //public class Foo : IComparable
    //{ }
    public class ContractClassActionWorkflow : StandardGenerateActionWorkflow
    {
        public ContractClassActionWorkflow(IconId icon)
            : base("ContractClass", icon, "Contract class", GenerateActionGroup.CLR_LANGUAGE, "Generate contract class",
                "Generates a contract class for selected item.", "Generate.ContractClass")
        {
        }

        public override double Order
        {
            get { return 1; }
        }

        public override bool IsAvailable(IDataContext dataContext)
        {
            var generatorContextFactory = 
                dataContext.GetData(DataConstants.SOLUTION)
                    .With(x => GeneratorManager.GetInstance(x))
                    .With(generatorManager => generatorManager.GetPsiLanguageFromContext(dataContext))
                    .With(languageType => LanguageManager.Instance.TryGetService<IGeneratorContextFactory>(languageType))
                    .Return(x => x);

            return generatorContextFactory != null;
        }
    }
}