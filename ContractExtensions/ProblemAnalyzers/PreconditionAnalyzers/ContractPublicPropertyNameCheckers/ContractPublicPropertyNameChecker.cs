using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Stages;
using JetBrains.ReSharper.Daemon.Stages.Dispatcher;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;
using ReSharper.ContractExtensions.Utilities;

[assembly: RegisterConfigurableSeverity(ContractPublicPropertyNameHighlighing.ServerityId,
  null,
  HighlightingGroupIds.CompilerWarnings, // this is actually a Code Contract compiler error CC1038
  ContractPublicPropertyNameHighlighing.ServerityId,
  "Warning for incorrect usage of the ContractPublicPropertyNameAttribute",
  Severity.ERROR,
  false)]


namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    /// <summary>
    /// Checks inconsistent visibility in Contract.Requires.
    /// </summary>
    /// <remarks>
    /// This class checks for the СС1010 error: 
    /// error CC1010: Field 'ConsoleApplication6.Demo.Analyzers.PreconditionWithContractPublicPropertyName._isValid' 
    /// is marked [ContractPublicPropertyName("IsValid")], but no public field/property named 'IsValid' with type 'System.Boolean' can be found
    /// </remarks>
    [ElementProblemAnalyzer(new[] { typeof(IFieldDeclaration) },
        HighlightingTypes = new[] { typeof(ContractPublicPropertyNameHighlighing) })]
    public class ContractPublicPropertyNameChecker : ElementProblemAnalyzer<IFieldDeclaration>
    {
        protected override void Run(IFieldDeclaration element, ElementProblemAnalyzerData data, 
            IHighlightingConsumer consumer)
        {

            var propertyNameAttribute = 
                element.DeclaredElement
                .With(x => 
                        x.GetAttributeInstances(
                            new ClrTypeName(typeof(ContractPublicPropertyNameAttribute).FullName), false)
                        .FirstOrDefault());

            if (propertyNameAttribute == null)
                return;

            var fieldOrPropertyName = (string)propertyNameAttribute.PositionParameter(0).ConstantValue.Value;
            var type = element.GetContainingTypeDeclaration();

            var fieldOrProperty = 
                type.MemberDeclarations
                    .FirstOrDefault(md => md.NameIdentifier.With(x => x.Name) == fieldOrPropertyName &&
                                          (md is IFieldDeclaration || md is IPropertyDeclaration));

            if (fieldOrProperty == null || fieldOrProperty.GetAccessRights() != AccessRights.PUBLIC)
            {
                consumer.AddHighlighting(
                    new ContractPublicPropertyNameHighlighing(element, fieldOrPropertyName, fieldOrProperty),
                    element.GetDocumentRange(), element.GetContainingFile());
            }
        }
    }
}