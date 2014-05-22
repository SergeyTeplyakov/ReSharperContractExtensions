using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Properties.CSharp;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Generate;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using JetBrains.Util.Special;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor
{
    class foof : IComparable
    {
        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class AddContractForExecutor
    {
        private readonly AddContractForAvailability _addContractForAvailability;
        private readonly ICSharpContextActionDataProvider _provider;
        private readonly CSharpElementFactory _factory;
        private readonly ICSharpFile _currentFile;
        private readonly IClassLikeDeclaration _classDeclaration;

        public AddContractForExecutor(AddContractForAvailability addContractForAvailability,
            ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(addContractForAvailability != null);
            Contract.Requires(addContractForAvailability.IsAvailable);
            Contract.Requires(provider != null);

            _addContractForAvailability = addContractForAvailability;
            _provider = provider;

            _factory = CSharpElementFactory.GetInstance(provider.PsiModule);
            // TODO: look at this class CSharpStatementNavigator

            _classDeclaration = provider.GetSelectedElement<IClassLikeDeclaration>(true, true);

            Contract.Assert(provider.SelectedElement != null);
            _currentFile = (ICSharpFile)provider.SelectedElement.GetContainingFile();

        }

        public void Execute(ISolution solution, IProgressIndicator progress)
        {
            // 1. Generate class that implements current interface or abstract class
            // 2. Add to the current type attribute ContractClass(typeof(NewlyGeneratedClass))
            // 3. Add to the new class attribute ContractClassFor(typeof(CurrentAbstractClassOrInterface))
            // 4.1 For interface: implement all members with throw new NotImplementedException()
            // 4.2 For abstract class: override all abstract members with throw new NotImplementedException();

            string contractClassName = CreateContractClassName();

            AddContractClassAttributeForCurrentType(contractClassName);

            var contractClass = GenerateContractClassDeclaration(contractClassName);

            ImplementInterfaceOrBaseClass(contractClass, progress);

            _currentFile.AddTypeDeclarationAfter(contractClass, _addContractForAvailability.TypeDeclaration);
        }

        private void ImplementInterfaceOrBaseClass(IClassDeclaration contractClass, IProgressIndicator progress)
        {
            
            string generateKind = GeneratorStandardKinds.Implementations;
            if (_addContractForAvailability.IsAbstractClass)
                generateKind = GeneratorStandardKinds.Overrides;

            using (var workflow = GeneratorWorkflowFactory.CreateWorkflowWithoutTextControl(
                generateKind,
                contractClass,
                contractClass))
                //_addContractForAvailability.TypeDeclaration))
            {
                Contract.Assert(workflow != null);

                

                workflow.Context.SetGlobalOptionValue(CSharpBuilderOptions.PropertyBody,
                    CSharpBuilderOptions.PropertyBodyDefault);

                workflow.Context.SetGlobalOptionValue(
                  CSharpBuilderOptions.ImplementationKind,
                  CSharpBuilderOptions.ImplementationKindExplicit);
                
                //var globalActions = workflow.Context.GlobalOptions;
                //var dd = globalActions
                //    .FirstOrDefault(g => g.ID == CSharpBuilderOptions.ImplementationKind);
                //if (dd != null)
                //{
                //    dd.OverridesGlobalOption = true;
                //    dd.Value = CSharpBuilderOptions.ImplementationKindPublicMember;
                //}

                workflow.Context.Anchor = contractClass;
                workflow.Context.InputElements.Clear();

                Contract.Assert(workflow.Context.ProvidedElements.Count != 0);
                workflow.Context.InputElements.AddRange(workflow.Context.ProvidedElements);
                //foreach (var e in workflow.Context.ProvidedElements)
                //{
                //    workflow.Context.InputElements.Add(e);
                //}


                //workflow.Context.GlobalOptions.Clear();
                
                

                

                workflow.GenerateAndFinish("Generate contract class", progress);
            }
        }

        private IClassDeclaration GenerateContractClassDeclaration(string contractClassName)
        {
            Contract.Ensures(Contract.Result<IClassDeclaration>() != null);

            var classDeclaration = (IClassDeclaration)_factory.CreateTypeMemberDeclaration(
                string.Format("class {0} {{}}", contractClassName));

            if (_addContractForAvailability.IsInterface)
            {
                AddContractClassForInterface(classDeclaration);
            }
            else
            {
                AddContractClassForAbstractClass(classDeclaration);
            }

            //ITypeUsage typeUsage = null;
            //var contractClass = CSharpTypeFactory.CreateType(typeUsage);
            return classDeclaration;
        }

        private void AddContractClassForAbstractClass(IClassDeclaration classDeclaration)
        {
            throw new System.NotImplementedException();
        }
        
        private void AddContractClassForInterface(IClassDeclaration classDeclaration)
        {
            Contract.Assert(_addContractForAvailability.InterfaceDeclaration.DeclaredElement != null);
            var interfaceType = TypeFactory.CreateType(_addContractForAvailability.InterfaceDeclaration.DeclaredElement);
            
            classDeclaration.AddSuperInterface(interfaceType, false);

            //var interfaceDeclaration = _addContractForAvailability.InterfaceDeclaration;
            //IClassMemberDeclaration previousDeclaration = null;
            //foreach (var memberDeclaration in interfaceDeclaration.ClassMemberDeclarations)
            //{
                
            //    classDeclaration.AddClassMemberDeclarationAfter(memberDeclaration, previousDeclaration);

            //    previousDeclaration = memberDeclaration;
            //}

        }

        [Pure]
        private string CreateContractClassName()
        {
            return _classDeclaration.DeclaredName + "Contract";
        }

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

        private void AddContractClassAttributeForCurrentType(string contractClassName)
        {
            var attribute = CreateContractClassAttribute(contractClassName);
            _classDeclaration.AddAttributeAfter(attribute, null);
        }
    }
}