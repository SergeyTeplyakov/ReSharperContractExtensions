using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Generate.Actions;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;
using DataConstants = JetBrains.ProjectModel.DataContext.DataConstants;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor.Generator
{
    [ActionHandler("Generate.ContractClass")]
    public class ContractClassAction : GenerateActionBase<ContractClassItemProvider>
    {
        protected override bool ShowMenuWithOneItem
        {
            get { return true; }
        }

        protected override RichText Caption
        {
            get { return "Generate Contract Class"; }
        }
    }

    [GenerateProvider]
    public class ContractClassItemProvider : IGenerateActionProvider
    {
        public IEnumerable<IGenerateActionWorkflow> CreateWorkflow(IDataContext dataContext)
        {
            var solution = dataContext.GetData(DataConstants.SOLUTION);
            Contract.Assert(solution != null);

            var iconManager = solution.GetComponent<PsiIconManager>();
            var icon = iconManager.GetImage(CLRDeclaredElementType.CLASS);
            return DoCreateWorkflow(icon);
        }

        private IEnumerable<IGenerateActionWorkflow> DoCreateWorkflow(IconId icon)
        {
            yield return new ContractClassActionWorkflow(icon);
        }
    }
}