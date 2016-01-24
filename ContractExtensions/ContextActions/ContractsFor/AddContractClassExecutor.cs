using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Settings;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor
{
    internal sealed class AddContractClassExecutor
    {
        private readonly AddContractClassAvailability _addContractForAvailability;
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly CSharpElementFactory _factory;
        private readonly ICSharpFile _currentFile;
        private readonly ICSharpFunctionDeclaration _requiredFunction;

        public AddContractClassExecutor(ICSharpContextActionDataProvider provider, AddContractClassAvailability addContractForAvailability, 
            ICSharpFunctionDeclaration requiredFunction = null)
        {
            Contract.Requires(provider != null);
            Contract.Requires(addContractForAvailability != null);
            Contract.Requires(addContractForAvailability.IsAvailable);

            _addContractForAvailability = addContractForAvailability;
            _provider = provider;
            _requiredFunction = requiredFunction;

            _factory = _provider.ElementFactory;
            // TODO: look at this class CSharpStatementNavigator

            Contract.Assert(provider.SelectedElement != null);
            
            _currentFile = (ICSharpFile)provider.SelectedElement.GetContainingFile();
        }

        public void Execute()
        {
            // If contract class already exists we can assume that it already "physical"
            // And we can implement it freely.

            IClassDeclaration contractClass = TryGetExistedContractClass();

            if (contractClass == null)
            {
                var newContractClass = CreateContractClass();

                AddContractClassForAttributeTo(newContractClass);

                contractClass = AddToPhysicalDeclaration(newContractClass);
                AddExcludeFromCodeCoverageAttributeIfNeeded(contractClass);

                AddContractClassAttributeIfNeeded(contractClass);
            }

            ImplementInterfaceOrBaseClass(contractClass);
            AddNonDefaultConstructorIfNeeded(contractClass);
        }

        private IClassDeclaration TryGetExistedContractClass()
        {
            return _addContractForAvailability.TypeDeclaration.GetContractClassDeclaration();
        }

        private IClassDeclaration CreateContractClass()
        {
            string contractClassName = GenerateUniqueContractClassName();

            IClassDeclaration newContractClass = GenerateContractClassDeclaration(contractClassName);

            return newContractClass;
        }

        private void AddContractClassForAttributeTo(IClassDeclaration contractClass)
        {
            var attribute = CreateContractClassForAttribute(_addContractForAvailability.TypeDeclaration);
            contractClass.AddAttributeAfter(attribute, null);
        }

        [Pure]
        private IAttribute CreateExcludeFromCodeCoverageAttribute()
        {
            ITypeElement type = TypeFactory.CreateTypeByCLRName(
                typeof(ExcludeFromCodeCoverageAttribute).FullName, _provider.PsiModule).GetTypeElement();

            return _factory.CreateAttribute(type);
        }

        private bool ShouldUseExcludeFromCodeCoverageAttribute()
        {
            var settings = _provider.SourceFile.GetSettingsStore()
                .GetKey<ContractExtensionsSettings>(SettingsOptimization.OptimizeDefault);

            return settings.UseExcludeFromCodeCoverageAttribute;
        }

        private void AddExcludeFromCodeCoverageAttributeIfNeeded(IClassDeclaration contractClass)
        {
            if (ShouldUseExcludeFromCodeCoverageAttribute())
            { 
                var attribute = CreateExcludeFromCodeCoverageAttribute();
                contractClass.AddAttributeAfter(attribute, null);
            }
        }

        private void ImplementContractForAbstractClass(IClassDeclaration contractClass,
            IClassDeclaration abstractClass)
        {
            Contract.Requires(contractClass != null);
            Contract.Requires(contractClass.DeclaredElement != null);
            Contract.Requires(abstractClass != null);
            Contract.Requires(abstractClass.DeclaredElement != null);

            using (var workflow = GeneratorWorkflowFactory.CreateWorkflowWithoutTextControl(
                GeneratorStandardKinds.Overrides,
                contractClass,
                abstractClass))
            {
                Contract.Assert(workflow != null);

                // By default this input for this workflow contains too many members:
                // It contains member from the base class (required) and
                // members from the other base classes (i.e. from System.Object).
                // Using some hack to get only members defined in the "abstractClass"

                // So I'm trying to find required elements myself.

                var missingMembers = contractClass.GetMissingMembersOf(abstractClass);
                
                if (_requiredFunction != null)
                {
                    var requiredDeclaration = _requiredFunction.DeclaredElement;
                    missingMembers = 
                        missingMembers.Where(x => GetMembers(x).Any(m => m.Equals(requiredDeclaration)))
                            .ToList();

                    Contract.Assert(missingMembers.Count != 0, "Should be at least one method to add!");
                }

                var membersToOverride =
                    missingMembers
                        .Select(x => new GeneratorDeclaredElement<IOverridableMember>(x.Member, x.Substitution))
                        .ToList();

                workflow.Context.InputElements.Clear();

                workflow.Context.ProvidedElements.Clear();
                workflow.Context.ProvidedElements.AddRange(membersToOverride);

                workflow.Context.InputElements.AddRange(workflow.Context.ProvidedElements);

                workflow.Generate("Generate contract class", NullProgressIndicator.Instance);
            }
        }

        private IEnumerable<IOverridableMember> GetMembers(OverridableMemberInstance overridableMember)
        {
            // For properties we should compare IProperty.Getter and IProperty.Setter with 
            // _requiredFunction.DeclaredElement
            if (overridableMember.Member is IProperty)
            {
                var property = (IProperty) overridableMember.Member;
                if (property.Getter != null)
                    yield return property.Getter;
                if (property.Setter != null)
                    yield return property.Setter;
            }
            else
            {
                yield return overridableMember.Member;
            }
        }

        private void ImplementContractForInterface(IClassDeclaration contractClass,
            IInterfaceDeclaration interfaceDeclaration)
        {
            Contract.Requires(contractClass != null);
            Contract.Requires(interfaceDeclaration != null);

            if (interfaceDeclaration.MemberDeclarations.Count == 0)
                return;

            using (var workflow = GeneratorWorkflowFactory.CreateWorkflowWithoutTextControl(
                GeneratorStandardKinds.MissingMembers,
                contractClass,
                interfaceDeclaration))
            {
                Contract.Assert(workflow != null);

                workflow.Context.InputElements.Clear();
                workflow.Context.InputElements.AddRange(workflow.Context.ProvidedElements);

                workflow.Context.SetOption(
                    CSharpBuilderOptions.ImplementationKind,
                    CSharpBuilderOptions.ImplementationKindExplicit);

                workflow.Generate("Generate contract class", NullProgressIndicator.Instance);
            }
        }

        private void ImplementInterfaceOrBaseClass(IClassDeclaration contractClass)
        {
            if (_addContractForAvailability.IsAbstractClass)
            {
                ImplementContractForAbstractClass(contractClass, _addContractForAvailability.ClassDeclaration);
            }
            else
            {
                ImplementContractForInterface(contractClass, _addContractForAvailability.InterfaceDeclaration);
            }
        }

        private void AddNonDefaultConstructorIfNeeded(IClassDeclaration contractClass)
        {
            if (!_addContractForAvailability.IsAbstractClass)
                return;

            var abstractBaseClass = _addContractForAvailability.ClassDeclaration;

            using (var workflow = GeneratorWorkflowFactory.CreateWorkflowWithoutTextControl(
                GeneratorStandardKinds.Constructor,
                contractClass,
                abstractBaseClass))
            {
                Contract.Assert(workflow != null);

                var ctor = 
                    workflow.Context.ProvidedElements
                    .OfType<GeneratorDeclaredElement<IConstructor>>()
                    .FirstOrDefault(c => !c.DeclaredElement.IsDefault);

                if (ctor != null)
                { 
                    workflow.Context.InputElements.Clear();
                    workflow.Context.InputElements.Add(ctor);
                    workflow.BuildOptions();
                    workflow.Generate("Generate missing constructor", NullProgressIndicator.Instance);
                }
            }

        }

        /// <summary>
        /// Adds <paramref name="contractClass"/> after implemented interface into the physical tree.
        /// </summary>
        /// <remarks>
        /// This method is absolutely crucial, because all R# "generate workflows" works correcntly
        /// only if TreeNode.IsPhysical() returns true.
        /// </remarks>
        private IClassDeclaration AddToPhysicalDeclaration(IClassDeclaration contractClass)
        {
            var holder = CSharpTypeAndNamespaceHolderDeclarationNavigator.GetByTypeDeclaration(
                _addContractForAvailability.TypeDeclaration);
            Contract.Assert(holder != null);
            
            var physicalContractClassDeclaration =
                (IClassDeclaration) holder.AddTypeDeclarationAfter(contractClass,
                    _addContractForAvailability.TypeDeclaration);

            return physicalContractClassDeclaration;
        }

        private IClassDeclaration GenerateContractClassDeclaration(string contractClassName)
        {
            Contract.Requires(contractClassName != null);
            Contract.Ensures(Contract.Result<IClassDeclaration>() != null);

            if (_addContractForAvailability.DeclaredType.IsOpenType)
                return GenerateGenericContractClassDeclaration(contractClassName);

            return GenerateNonGenericContractClassDeclaration(contractClassName);
        }

        private IClassDeclaration GenerateNonGenericContractClassDeclaration(string contractClassName)
        {
            return (IClassDeclaration)_factory.CreateTypeMemberDeclaration(
                "abstract class $0 : $1 {}", 
                contractClassName, _addContractForAvailability.DeclaredType);
        }

        private IClassDeclaration GenerateGenericContractClassDeclaration(string contractClassName)
        {
            // This solution was found at CreateDerivedTypeAction.cs from decompiled R# SDK
            var baseTypeElement = _addContractForAvailability.DeclaredType.GetTypeElement();
            Contract.Assert(baseTypeElement != null);

            string typeDeclaration = 
                baseTypeElement.TypeParameters
                .AggregateString(",", (builder, parameter) => builder.Append(parameter.ShortName));
            typeDeclaration = "<" + typeDeclaration + ">";

            var classDeclaration = (IClassDeclaration)_factory.CreateTypeMemberDeclaration(
                "abstract class $0 " + typeDeclaration + " : $1" + typeDeclaration + " {}",
                new object[] {contractClassName, baseTypeElement});

            var map = new Dictionary<ITypeParameter, IType>();
            for (int i = 0; i < baseTypeElement.TypeParameters.Count; i++)
            {
                ITypeParameterOfTypeDeclaration declaration3 = classDeclaration.TypeParameters[i];
                ITypeParameter key = baseTypeElement.TypeParameters[i];
                map.Add(key, TypeFactory.CreateType(declaration3.DeclaredElement));
            }
            ISubstitution substitution = EmptySubstitution.INSTANCE.Extend(map);
            
            for (int j = 0; j < baseTypeElement.TypeParameters.Count; j++)
            {
                ITypeParameter typeParameter = baseTypeElement.TypeParameters[j];
                ITypeParameterOfTypeDeclaration declaration4 = classDeclaration.TypeParameters[j];
                ITypeParameterConstraintsClause clause = _factory.CreateTypeParameterConstraintsClause(typeParameter, substitution, declaration4.DeclaredName);
                if (clause != null)
                {
                    classDeclaration.AddTypeParameterConstraintsClauseBefore(clause, null);
                }
            }
            
            return classDeclaration;
        }

        [Pure]
        private string GenerateUniqueContractClassName()
        {
            // TODO: will be implemented later. Don't know how to find type in the namespace!
            return _addContractForAvailability.TypeDeclaration.DeclaredName + "Contract";
        }

        // TODO: controlflow said that IAttribute is not the best way to do this!
        [Pure]
        private IAttribute CreateContractClassAttribute(IClassDeclaration contractClass)
        {
            ITypeElement attributeType = TypeFactory.CreateTypeByCLRName(
                typeof(ContractClassAttribute).FullName, _provider.PsiModule).GetTypeElement();

            var declaredType = contractClass.DeclaredElement;
            var typeofExpression = _factory.CreateExpression("typeof($0)", declaredType);

            var attribute = _factory.CreateAttribute(attributeType);

            attribute.AddArgumentAfter(
                _factory.CreateArgument(ParameterKind.VALUE, typeofExpression),
                null);

            return attribute;
        }

        [Pure]
        private IAttribute CreateContractClassForAttribute(IClassLikeDeclaration contractClassFor)
        {
            var declaredType = _addContractForAvailability.DeclaredType.GetTypeElement();
            ITypeElement type = TypeFactory.CreateTypeByCLRName(
                typeof(ContractClassForAttribute).FullName, _provider.PsiModule).GetTypeElement();

            var expression = _factory.CreateExpression("typeof($0)", declaredType);

            var attribute = _factory.CreateAttribute(type);

            attribute.AddArgumentAfter(
                _factory.CreateArgument(ParameterKind.VALUE, expression),
                null);

            return attribute;
        }

        private void AddContractClassAttributeIfNeeded(IClassDeclaration contractClass)
        {
            if (!_addContractForAvailability.TypeDeclaration.HasAttribute(typeof (ContractClassAttribute)))
            {
                var attribute = CreateContractClassAttribute(contractClass);
                _addContractForAvailability.TypeDeclaration.AddAttributeAfter(attribute, null);
            }
        }
    }
}