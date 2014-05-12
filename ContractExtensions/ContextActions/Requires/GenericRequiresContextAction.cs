using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public sealed class GenericRequiresContextAction : RequiresContextActionBase
    {
        private const string Name = "Add Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Requires '{0}' is not null (with Contract.Requires<ArgumentNullException>)";

        public GenericRequiresContextAction(ICSharpContextActionDataProvider provider)
            : base(provider)
        {}

        public override string Text
        {
            get { return string.Format(Format, _currentParameterName); }
        }

        protected override ICSharpStatement CreateContractRequires(CSharpElementFactory factory,
                                            IParameterDeclaration parameterDeclaration)
        {
            // TODO: add System using if necessary!
            string stringStatement = string.Format("{0}<ArgumentNullException>.Requires({1} != null);",
                typeof(Contract).Name, parameterDeclaration.DeclaredName);
            var statement = factory.CreateStatement(stringStatement);
            return statement;
        }
    }
}