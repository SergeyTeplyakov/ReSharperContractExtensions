using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Returns contract method for specified <paramref name="functionDeclaration"/>.
        /// </summary>
        /// <remarks>
        /// This helper method works correctly for both: abstract member functions and for
        /// abstract property getters or setters.
        /// </remarks>
        [System.Diagnostics.Contracts.Pure]
        public static ICSharpFunctionDeclaration GetContractMethodForAbstractFunction(this ICSharpFunctionDeclaration functionDeclaration)
        {
            Contract.Requires(functionDeclaration != null);
            
            Contract.Requires(functionDeclaration.IsAbstract);

            var currentClass = (IClassLikeDeclaration)functionDeclaration.GetContainingTypeDeclaration();
            Contract.Assert(currentClass != null);

            var contractClassDeclaration = currentClass.GetContractClassDeclaration();

            if (contractClassDeclaration == null || contractClassDeclaration.Body == null)
                return null;

            // Get all "function declarations": methods and property accessors
            var functionDeclarations =
                contractClassDeclaration.Body.Methods.OfType<ICSharpFunctionDeclaration>()
                    .Concat(
                        contractClassDeclaration.Body.Properties
                            .SelectMany(pd => pd.AccessorDeclarations)
                    );

            // Function declaration contains IFunction as DeclaredElement, but we need IOverridableMember
            // instead to get all super members
            Func<ICSharpFunctionDeclaration, bool> isOverridesCurrentFunction =
                md => md.DeclaredElement.With(x => x as IOverridableMember)
                    .Return(x => x.GetAllSuperMembers(), Enumerable.Empty<OverridableMemberInstance>())
                    .SelectMany(GetMembers)
                    .Any(overridable => overridable.Equals(functionDeclaration.DeclaredElement));

            return functionDeclarations.FirstOrDefault(isOverridesCurrentFunction);

            //Func<IMethodDeclaration, bool> isOverridesCurrentFunction =
            //    md => md.DeclaredElement.GetAllSuperMembers()
            //        .SelectMany(GetMembers)
            //        .Any(overridable => overridable.Equals(functionDeclaration.DeclaredElement));

            //Func<IPropertyDeclaration, bool> isOverridesCurrentProperty =
            //    pd => pd.DeclaredElement.GetAllSuperMembers()
            //        .SelectMany(GetMembers)
            //        .Any(overridable => overridable.Equals(functionDeclaration.DeclaredElement));

            ////IPropertyDeclaration pd = null;
            ////var rrr = pd.DeclaredElement.Getter as ICSharpFunctionDeclaration;
            //IMethodDeclaration met = null;
            //IPropertyDeclaration pppd = null;
            //IClassMemberDeclaration ccm = met;
            //IAccessorDeclaration ad = null;
            //var function = contractClassDeclaration
            //    .With(x => x.Body)
            //    .Return(x => x.Methods.FirstOrDefault(isOverridesCurrentFunction));
            //if (function != null)
            //    return function;

            //var selectedProperty = contractClassDeclaration.With(x => x.Body)
            //    .Return(x => x.Properties.FirstOrDefault(isOverridesCurrentProperty));
            //return selectedProperty.AccessorDeclarations.FirstOrDefault();
            ////var rrr = selectedProperty.DeclaredElement.Getter as ICSharpFunctionDeclaration;
            //return null;
            ////return selectedProperty.DeclaredElement;
        }


        private static IEnumerable<IMethod> GetMethodsAndProperties(IClassBody classBody)
        {
            var methods = classBody.Methods.Select(m => m.DeclaredElement);

            var properties =
                classBody.Properties
                    .SelectMany(p => new[] {p.DeclaredElement.Getter, p.DeclaredElement.Setter})
                    .Where(m => m != null);

            return methods.Concat(properties);
        }

        

        private static IEnumerable<IOverridableMember> GetMembers(OverridableMemberInstance overridableMember)
        {
            // For properties we should compare IProperty.Getter and IProperty.Setter with 
            // _requiredFunction.DeclaredElement
            if (overridableMember.Member is IProperty)
            {
                var property = (IProperty)overridableMember.Member;
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
    }
}