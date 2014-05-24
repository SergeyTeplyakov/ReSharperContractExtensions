using System;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ContractUtils
{
    public static class ContractExtensions
    {
        public static bool HasContract(this IClassLikeDeclaration classLikeDeclaration)
        {
            return GetContractClassDeclaration(classLikeDeclaration) != null;
        }

        [CanBeNull]
        public static IClassDeclaration GetContractClassDeclaration(this IClassLikeDeclaration classLikeDeclaration)
        {
            Contract.Requires(classLikeDeclaration != null);
            Contract.Requires(classLikeDeclaration.DeclaredElement != null);

            var contractClassAttributeType = new ClrTypeName(typeof(ContractClassAttribute).FullName);
            var contractClassAttributeInstance =
                classLikeDeclaration.DeclaredElement.GetAttributeInstances(contractClassAttributeType, true)
                    .FirstOrDefault();

            if (contractClassAttributeInstance == null)
                return null;

            Contract.Assert(contractClassAttributeInstance.PositionParameterCount == 1);

            return contractClassAttributeInstance.PositionParameters()
                .FirstOrDefault()
                .With(x => x.TypeValue)
                .With(x => x.GetScalarType())
                .With(x => x.Resolve())
                .With(x => x.DeclaredElement)
                .With(x => x.GetDeclarations().FirstOrDefault())
                .Return(x => x as IClassDeclaration);
        }

        /// <summary>
        /// "Target" function (where preconditions should recided) could be different from the selected
        /// fuction.
        /// </summary>
        /// <remarks>
        /// This is true for preconditions for abstract methods and interface methods. In this case
        /// preconditions should be placed into the special "contract class".
        /// </remarks>
        [CanBeNull]
        public static ICSharpFunctionDeclaration GetContractFunction(this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            Contract.Requires(functionDeclaration.DeclaredElement != null);

            if (functionDeclaration.IsAbstract)
                return GetContractMethodForAbstractFunction(functionDeclaration);

            return functionDeclaration;
        }

        [System.Diagnostics.Contracts.Pure]
        private static ICSharpFunctionDeclaration GetContractMethodForAbstractFunction(ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            
            Contract.Requires(functionDeclaration.IsAbstract);

            var currentClass = (IClassLikeDeclaration)functionDeclaration.GetContainingTypeDeclaration();
            Contract.Assert(currentClass != null);

            var contractClassDeclaration = currentClass.GetContractClassDeclaration();

            Func<IMethodDeclaration, bool> isOverridesCurrentFunction =
                md => md.DeclaredElement.GetAllSuperMembers()
                    .Any(overridable => overridable.Member.Equals(functionDeclaration.DeclaredElement));

            return contractClassDeclaration
                .With(x => x.Body)
                .Return(x => x.Methods.FirstOrDefault(isOverridesCurrentFunction));
        }

    }
}