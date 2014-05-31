using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor
{
    internal sealed class AddContractExecutor
    {
        private readonly AddContractAvailability _addContractForAvailability;
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly CSharpElementFactory _factory;
        private readonly ICSharpFile _currentFile;
        private readonly ICSharpFunctionDeclaration _requiredFunction;

        public AddContractExecutor(ICSharpContextActionDataProvider provider, AddContractAvailability addContractForAvailability, 
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

        public void Execute(ISolution solution, IProgressIndicator progress)
        {
            // If contract class already exists we can assume that it already "physical"
            // And we can implement it freely.

            IClassDeclaration contractClass = TryGetExistedContractClass();

            if (contractClass == null)
            {
                var newContractClass = CreateContractClass();

                AddContractClassAttributeIfNeeded(newContractClass.DeclaredName);
                AddContractClassForAttributeTo(newContractClass);

                contractClass = AddToPhysicalDeclaration(newContractClass);
            }

            ImplementInterfaceOrBaseClass(contractClass);
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
            var attribute = CreateContractClassForAttribute(
                _addContractForAvailability.TypeDeclaration.DeclaredName);
            contractClass.AddAttributeAfter(attribute, null);
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
                }

                var membersToOverride =
                    missingMembers
                        .Select(x => new GeneratorDeclaredElement<IOverridableMember>(x.Member, x.Substitution))
                        .ToList();

                Contract.Assert(membersToOverride.Count != 0, "Should be at least one method to add!");

                workflow.Context.InputElements.Clear();

                workflow.Context.ProvidedElements.Clear();
                workflow.Context.ProvidedElements.AddRange(membersToOverride);

                workflow.Context.InputElements.AddRange(workflow.Context.ProvidedElements);

                workflow.GenerateAndFinish("Generate contract class", NullProgressIndicator.Instance);
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
                GeneratorStandardKinds.Implementations,
                contractClass,
                interfaceDeclaration))
            {
                Contract.Assert(workflow != null);

                workflow.Context.SetGlobalOptionValue(
                    CSharpBuilderOptions.ImplementationKind,
                    CSharpBuilderOptions.ImplementationKindExplicit);

                workflow.Context.InputElements.Clear();
                workflow.Context.InputElements.AddRange(workflow.Context.ProvidedElements);

                workflow.GenerateAndFinish("Generate contract class", NullProgressIndicator.Instance);
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

            var classDeclaration = (IClassDeclaration)_factory.CreateTypeMemberDeclaration(
                string.Format("abstract class {0} : $0 {{}}", contractClassName), 
                _addContractForAvailability.DeclaredType);

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
        private IAttribute CreateContractClassAttribute(string contractClassName)
        {
            ITypeElement type = TypeFactory.CreateTypeByCLRName(
                typeof(ContractClassAttribute).FullName,
                _provider.PsiModule, _currentFile.GetResolveContext()).GetTypeElement();

            var expression = _factory.CreateExpressionAsIs(
                string.Format("typeof({0})", contractClassName));

            var attribute = _factory.CreateAttribute(type);

            attribute.AddArgumentAfter(
                _factory.CreateArgument(ParameterKind.VALUE, expression),
                null);

            return attribute;
        }

        [Pure]
        private IAttribute CreateContractClassForAttribute(string contractClassForName)
        {
            ITypeElement type = TypeFactory.CreateTypeByCLRName(
                typeof(ContractClassForAttribute).FullName,
                _provider.PsiModule, _currentFile.GetResolveContext()).GetTypeElement();

            var expression = _factory.CreateExpressionAsIs(
                string.Format("typeof({0})", contractClassForName));

            var attribute = _factory.CreateAttribute(type);

            attribute.AddArgumentAfter(
                _factory.CreateArgument(ParameterKind.VALUE, expression),
                null);

            return attribute;
        }

        private void AddContractClassAttributeIfNeeded(string contractClassName)
        {
            if (!_addContractForAvailability.TypeDeclaration.HasAttribute(typeof (ContractClassAttribute)))
            {
                var attribute = CreateContractClassAttribute(contractClassName);
                _addContractForAvailability.TypeDeclaration.AddAttributeAfter(attribute, null);
            }
        }
    }
}