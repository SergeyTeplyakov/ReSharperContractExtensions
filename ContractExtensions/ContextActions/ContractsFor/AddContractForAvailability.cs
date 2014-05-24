using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContextActions.ContractsFor
{
    internal sealed class AddContractForAvailability
    {
        public static readonly AddContractForAvailability Unavailable = new AddContractForAvailability {IsAvailable = false};

        private readonly ICSharpContextActionDataProvider _provider;
        private readonly bool _enableInsideClassLikeDeclaration;

        private AddContractForAvailability()
        {}

        public AddContractForAvailability(ICSharpContextActionDataProvider provider,
            bool enableInsideClassLikeDeclaration)
        {
            Contract.Requires(provider != null);
            Contract.Requires(provider.SelectedElement != null);

            _provider = provider;
            _enableInsideClassLikeDeclaration = enableInsideClassLikeDeclaration;

            IsAvailable = ComputeIsAvailable();
            if (IsAvailable)
            {
                ComputeSelectedMember();
            }
        }

        private void ComputeSelectedMember()
        {
            InterfaceDeclaration = _provider.GetSelectedElement<IInterfaceDeclaration>(true, true);

            if (InterfaceDeclaration == null)
                ClassDeclaration = _provider.GetSelectedElement<IClassDeclaration>(true, true);

            SelectedDeclaration = GetSelectedTypeName();
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!IsAvailable || _provider != null);
            Contract.Invariant(!IsAvailable || SelectedDeclaration != null);
            Contract.Invariant(!IsAvailable || (IsInterface || IsAbstractClass));
            Contract.Invariant(!IsAvailable || (IsInterface || ClassDeclaration.IsAbstract));
            Contract.Invariant(!IsAvailable || TypeDeclaration != null);
        }

        private bool ComputeIsAvailable()
        {
            if (_enableInsideClassLikeDeclaration)
            {
                // In this case action should be enabled even if the caret
                // is inside class-like declaration (i.e. inside class or interface)
                if (!_provider.IsSelected<IClassLikeDeclaration>())
                    return false;
            }
            else
            {
                if (!IsInterfaceDeclarationSelected() && !IsAbstractClassDeclarationSelected())
                    return false;
            }

            if (ContractClassAlreadyDefined())
                return false;

            return true;
        }

        private bool ContractClassAlreadyDefined()
        {
            var classDeclaration = _provider.GetSelectedElement<IClassLikeDeclaration>(true, true);

            return classDeclaration.HasAttribute(typeof (ContractClassAttribute));
        }

        private bool IsAbstractClassDeclarationSelected()
        {
            var classDeclartion = _provider.GetSelectedElement<IClassDeclaration>(true, true);

            if (classDeclartion == null)
                return false;

            // See the comment at IsInterfaceDeclarationSelected method.
            if (!_provider.IsSelected<IIdentifier>() || !(_provider.SelectedElement.PrevSibling is IIdentifier))
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
            if (_provider.IsSelected<IIdentifier>() || _provider.SelectedElement.PrevSibling is IIdentifier)
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
        public string SelectedDeclaration { get; private set; }

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