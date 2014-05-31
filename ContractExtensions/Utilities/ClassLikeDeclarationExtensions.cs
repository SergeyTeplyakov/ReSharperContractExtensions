using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.ReSharper.Feature.Services.Generate;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace ReSharper.ContractExtensions.Utilities
{
    internal static class ClassLikeDeclarationExtensions
    {
        public static bool HasAttribute(this IClassLikeDeclaration classLikeDeclaration, Type attributeType)
        {
            Contract.Requires(classLikeDeclaration != null);
            Contract.Requires(classLikeDeclaration.DeclaredElement != null);
            Contract.Requires(attributeType != null);
            Contract.Requires(attributeType.IsAttribute());

            var clrAttribute = new ClrTypeName(attributeType.FullName);
            return classLikeDeclaration.DeclaredElement.HasAttributeInstance(
                new ClrTypeName(attributeType.FullName), inherit: true);
        }

        public static bool Overrides(this IClassLikeDeclaration classLikeDeclaration,
            ICSharpFunctionDeclaration baseFunctionDeclaration)
        {
            Contract.Requires(classLikeDeclaration != null);
            Contract.Requires(baseFunctionDeclaration != null);

            Func<IMethodDeclaration, bool> isOverridesCurrentFunction =
                md => md.DeclaredElement.GetAllSuperMembers()
                    .Any(overridable => overridable.Member.Equals(baseFunctionDeclaration.DeclaredElement));

            return classLikeDeclaration
                .With(x => x.Body)
                .Return(x => x.Methods.FirstOrDefault(isOverridesCurrentFunction)) != null;
        }

        /// <summary>
        /// Return list of members from the <paramref name="declaration"/>, that overridable from its base class 
        /// (<paramref name="baseDeclaration"/>).
        /// </summary>
        public static List<OverridableMemberInstance> GetMissingMembersOf(
            this IClassLikeDeclaration declaration,
            IClassLikeDeclaration baseDeclaration)
        {
            Contract.Requires(declaration != null);
            Contract.Requires(declaration.DeclaredElement != null);
            Contract.Requires(baseDeclaration != null);
            Contract.Requires(baseDeclaration.DeclaredElement != null);

            Contract.Ensures(Contract.Result<List<OverridableMemberInstance>>() != null);

            var potentialOverrides =
                GenerateUtil.GetOverridableMembersOrder(declaration.DeclaredElement, false)
                    .Where(e => e.DeclaringType.GetClrName().FullName ==
                            baseDeclaration.DeclaredElement.GetClrName().FullName)
                    // This code provides two elements for property! So I'm trying to remove another instance!
                    .Where(x => !x.Member.ShortName.StartsWith("get_"))
                    .Where(x => !x.Member.ShortName.StartsWith("set_"))
                    .ToList();

            var alreadyOverriden = new HashSet<OverridableMemberInstance>(
                declaration.GetOverridenMembers());

            var notOverridenMembers = new List<OverridableMemberInstance>();
            foreach (var member in potentialOverrides)
            {
                if (!alreadyOverriden.Contains(member))
                    notOverridenMembers.Add(member);
            }

            return notOverridenMembers;
        }

        public static IEnumerable<OverridableMemberInstance> GetOverridenMembers(
            this IClassLikeDeclaration classLikeDeclaration)
        {
            Contract.Requires(classLikeDeclaration != null);
            Contract.Ensures(Contract.Result<IEnumerable<OverridableMemberInstance>>() != null);

            var alreadyOverriden = new List<OverridableMemberInstance>();

            foreach (IOverridableMember member in classLikeDeclaration.DeclaredElement.GetMembers().OfType<IOverridableMember>())
            {
                if (member.IsExplicitImplementation)
                {
                    foreach (IExplicitImplementation implementation in member.ExplicitImplementations)
                    {
                        OverridableMemberInstance element = implementation.Resolve();
                        if (element != null)
                        {
                            alreadyOverriden.Add(element);
                        }
                    }
                }
                else if (member.IsOverride)
                {
                    foreach (OverridableMemberInstance isntance in new OverridableMemberInstance(member).GetImmediateOverride())
                    {
                        alreadyOverriden.Add(isntance);
                    }
                }
            }

            return alreadyOverriden;
        }
    }
}