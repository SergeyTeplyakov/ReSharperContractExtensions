using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.ContractUtils;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor
{
    // TODO: this class is too complex. Please split it 
    // to AddContractToSelectedTypeAvailability and AddContractToSelectedFunctionAvailability
    internal sealed class AddContractClassAvailability
    {
        public static readonly AddContractClassAvailability Unavailable = new AddContractClassAvailability {IsAvailable = false};

        private readonly ICSharpContextActionDataProvider _provider;
        private readonly bool _contractForSelectedMethod;
        private readonly ICSharpFunctionDeclaration _selectedFunctionDeclaration;

        private AddContractClassAvailability()
        {}

        private AddContractClassAvailability(ICSharpContextActionDataProvider provider,
            bool contractForSelectedMethod, ICSharpFunctionDeclaration selectedFunctionDeclaration)
        {
            Contract.Requires(provider != null);
            Contract.Requires(provider.SelectedElement != null);

            _provider = provider;
            _contractForSelectedMethod = contractForSelectedMethod;
            _selectedFunctionDeclaration = selectedFunctionDeclaration ?? GetSelectedFunction();

            IsAvailable = ComputeIsAvailable();
            if (IsAvailable)
            {
                ComputeSelectedDeclarationMember();
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || SelectedDeclarationTypeName != null);
            Contract.Invariant(!IsAvailable || (IsInterface || IsAbstractClass));
            Contract.Invariant(!IsAvailable || (IsInterface || ClassDeclaration.IsAbstract));
            Contract.Invariant(!IsAvailable || TypeDeclaration != null);
        }

        public static AddContractClassAvailability IsAvailableForSelectedType(ICSharpContextActionDataProvider provider)
        {
            Contract.Requires(provider != null);
            Contract.Ensures(Contract.Result<AddContractClassAvailability>() != null);

            return new AddContractClassAvailability(provider, false, null);
        }

        public static AddContractClassAvailability IsAvailableForSelectedMethod(ICSharpContextActionDataProvider provider, ICSharpFunctionDeclaration selectedFunction = null)
        {
            Contract.Requires(provider != null);
            Contract.Ensures(Contract.Result<AddContractClassAvailability>() != null);

            return new AddContractClassAvailability(provider, true, selectedFunction);
        }

        private void ComputeSelectedDeclarationMember()
        {
            InterfaceDeclaration = _provider.GetSelectedElement<IInterfaceDeclaration>(true, true);

            if (InterfaceDeclaration == null)
                ClassDeclaration = _provider.GetSelectedElement<IClassDeclaration>(true, true);

            SelectedDeclarationTypeName = GetSelectedTypeName();
        }

        private bool ComputeIsAvailable()
        {
            if (_contractForSelectedMethod)
            {
                // make no sense generate contract for selected method if the
                // method is not selected!
                if (!IsSelectedFunctionAbstract())
                    return false; 

                // In this case action should be enabled even (and only) if the caret
                // is inside class-like declaration (i.e. inside class or interface)
                
                // TODO: not sure that this check needed any more! 
                // If the selected fuction is abstract then we definetely in the class or interface, right?
                // But maybe I
                if (!InsideClassOrInterfaceDeclaration())
                    return false;

                if (SelectedMethodAlreadyHasContract())
                    return false;
            }
            else
            {
                if (!IsInterfaceDeclarationSelected() && !IsAbstractClassDeclarationSelected())
                    return false;

                if (ContractClassAlreadyFullyDefined())
                    return false;
            }

            return true;
        }

        [Pure] private ICSharpFunctionDeclaration GetSelectedFunction()
        {
            return _provider.GetSelectedElement<ICSharpFunctionDeclaration>(true, true);
        }

        [Pure] private bool IsSelectedFunctionAbstract()
        {
            if (_selectedFunctionDeclaration == null)
                return false;
            return _selectedFunctionDeclaration.IsAbstract;
        }

        [Pure] private bool SelectedMethodAlreadyHasContract()
        {
            Contract.Assert(_selectedFunctionDeclaration != null);

            return _selectedFunctionDeclaration.GetContractMethodForAbstractFunction() != null;
        }

        private bool ContractClassAlreadyFullyDefined()
        {
            var classDeclaration = _provider.GetSelectedElement<IClassLikeDeclaration>(true, true);

            var contractClass = classDeclaration.GetContractClassDeclaration();
            if (contractClass == null)
                return false;

            // This action should be enabled if contract class exists but does not
            // implement all abstract members from the base class (or interface).
            return contractClass.GetMissingMembersOf(classDeclaration).Count == 0;
        }

        private bool InsideClassOrInterfaceDeclaration()
        {
            return _provider.IsSelected<IClassLikeDeclaration>();
        }

        private bool IsAbstractClassDeclarationSelected()
        {
            var classDeclartion = _provider.GetSelectedElement<IClassDeclaration>(true, true);

            if (classDeclartion == null)
                return false;

            // See the comment at IsInterfaceDeclarationSelected method.
            if ((!_provider.IsSelected<IIdentifier>() && !(_provider.SelectedElement.PrevSibling is IIdentifier)) 
                || _provider.IsSelected<IClassBody>()) // not inside class or interface declarations!
                return false;

            return classDeclartion.IsAbstract;
        }

        private bool IsInterfaceDeclarationSelected()
        {
            if (!_provider.IsSelected<IInterfaceDeclaration>())
                return false;

            // Caret could be on the middle of the identifier: interface f{caret}oo {}
            // or at the end of it: interface foo{caret} {}.
            // In second case _provider.IsSelected<IIdentifier>() will be false
            if ((_provider.IsSelected<IIdentifier>() || _provider.SelectedElement.PrevSibling is IIdentifier)
                && !_provider.IsSelected<IClassBody>()) // not inside class declaration!
                return true;
            
            return false;
        }

        private string GetSelectedTypeName()
        {
            // Interface and class declarations has common ancestor - ITypeMemberDeclaration
            var declaration = _provider.GetSelectedElement<ITypeMemberDeclaration>(true, true);
            Contract.Assert(declaration != null);

            Contract.Assert(declaration.DeclaredElement != null);

            return declaration.DeclaredElement.ShortName;
        }

        public bool IsAvailable { get; private set; }
        public string SelectedDeclarationTypeName { get; private set; }

        public IInterfaceDeclaration InterfaceDeclaration { get; private set; }
        public IClassDeclaration ClassDeclaration { get; private set; }

        public bool IsInterface { get { return InterfaceDeclaration != null; } }
        public bool IsAbstractClass { get { return ClassDeclaration != null; } }

        public IClassLikeDeclaration TypeDeclaration
        {
            get
            {
                if (IsInterface)
                    return InterfaceDeclaration;
                return ClassDeclaration;
            }
        }

        public IDeclaredType DeclaredType
        {
            get
            {
                // TODO: should I add this to object invariant?
                Contract.Assert(TypeDeclaration.DeclaredElement != null);
                return TypeFactory.CreateType(TypeDeclaration.DeclaredElement);
            }
        }
    }
}