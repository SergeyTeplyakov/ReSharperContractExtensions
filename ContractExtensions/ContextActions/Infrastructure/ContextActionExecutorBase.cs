using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Impl.Types;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.Lazy;

namespace ReSharper.ContractExtensions.ContextActions.Infrastructure
{
    /// <summary>
    /// Abstract base class for executing context actions.
    /// </summary>
    internal abstract class ContextActionExecutorBase
    {
        protected readonly ICSharpContextActionDataProvider _provider;

        protected readonly CSharpElementFactory _factory;
        protected readonly ICSharpFile _currentFile;

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

            _provider = provider;

            _factory = _provider.ElementFactory;

            Contract.Assert(_provider.SelectedElement != null,
                "Can't create executor if SelectedElement is null");

            _currentFile = (ICSharpFile)_provider.SelectedElement.GetContainingFile();

        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_provider != null);
            Contract.Invariant(_factory != null);
            Contract.Invariant(_currentFile != null);
        }

        public abstract void ExecuteTransaction();

        [Pure]
        protected IDeclaredType CreateDeclaredType(IClrTypeName clrTypeName)
        {
            Contract.Requires(clrTypeName != null);
            Contract.Ensures(Contract.Result<IDeclaredType>() != null);

            return new DeclaredTypeFromCLRName(clrTypeName, _provider.PsiModule, _provider.SourceFile.ResolveContext);
        }

        [Pure]
        protected IDeclaredType CreateDeclaredType(System.Type type)
        {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<IDeclaredType>() != null);

            return CreateDeclaredType(new ClrTypeName(type.FullName));
        }

        [Pure]
        protected PredefinedType GetPredefinedType()
        {
            Contract.Ensures(Contract.Result<PredefinedType>() != null);
            return _provider.PsiModule.GetPredefinedType(
                _provider.SourceFile.ResolveContext);
        }

        public IDeclaredType ContractType
        {
            get { return CreateDeclaredType(typeof (Contract)); }
        }

    }
}