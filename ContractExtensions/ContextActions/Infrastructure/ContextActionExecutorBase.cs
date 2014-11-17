using System.Diagnostics.Contracts;
using JetBrains.Application.Settings.Storage.Persistence;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.Types;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.Lazy;

namespace ReSharper.ContractExtensions.ContextActions.Infrastructure
{
    /// <summary>
    /// Abstract base class for executing context actions.
    /// </summary>
    internal abstract class ContextActionExecutorBase
    {
        //protected readonly ICSharpContextActionDataProvider _provider;

        protected readonly CSharpElementFactory _factory;
        protected readonly ICSharpFile _currentFile;
        private readonly IPsiServices _psiServices;
        private readonly IPsiModule _psiModule;

        protected ContextActionExecutorBase(ICSharpStatement statement)
        {
            Contract.Requires(statement != null);

            _psiModule = statement.GetPsiModule();
            _psiServices = statement.GetPsiServices();
            _factory = CSharpElementFactory.GetInstance(statement);
            _currentFile = (ICSharpFile)statement.GetContainingFile();
        }

        protected ContextActionExecutorBase(ContextActionAvailabilityBase availability)
            : this(availability.Provider)
        {
            Contract.Requires(availability != null);
            Contract.Requires(availability.IsAvailable, 
                "Executor should get available context action");
        }

        protected ContextActionExecutorBase(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);


            _factory = provider.ElementFactory;

            Contract.Assert(provider.SelectedElement != null,
                "Can't create executor if SelectedElement is null");

            _currentFile = (ICSharpFile)provider.SelectedElement.GetContainingFile();
            _psiServices = provider.PsiServices;
            _psiModule = provider.PsiModule;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_factory != null);
            Contract.Invariant(_currentFile != null);
            Contract.Invariant(_psiServices != null);
            Contract.Invariant(_psiModule != null);
        }

        public void ExecuteTransaction()
        {
            DoExecuteTransaction();
            _psiServices.Caches.Update();
        }

        protected abstract void DoExecuteTransaction();

        [Pure]
        protected ITypeElement CreateDeclaredType(IClrTypeName clrTypeName)
        {
            Contract.Requires(clrTypeName != null);
            Contract.Ensures(Contract.Result<ITypeElement>() != null);

            return new DeclaredTypeFromCLRName(clrTypeName, _psiModule, _currentFile.GetResolveContext())
                .GetTypeElement();
        }

        [Pure]
        protected ITypeElement CreateDeclaredType(System.Type type)
        {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<ITypeElement>() != null);

            return CreateDeclaredType(new ClrTypeName(type.FullName));
        }

        [Pure]
        protected PredefinedType GetPredefinedType()
        {
            Contract.Ensures(Contract.Result<PredefinedType>() != null);
            return _psiModule.GetPredefinedType(
                _currentFile.GetResolveContext());
        }

        public ITypeElement ContractType
        {
            get
            {
                Contract.Ensures(Contract.Result<ITypeElement>() != null);
                return CreateDeclaredType(typeof (Contract));
            }
        }

    }
}