using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using JetBrains.ReSharper.Psi;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    internal static class FuncEx
    {
        public static Func<T, bool> Or<T>(this Func<T, bool> original, Func<T, bool> another)
        {
            return t => original(t) || another(t);
        }

        public static Func<T, bool> And<T>(this Func<T, bool> original, Func<T, bool> another)
        {
            return t => original(t) && another(t);
        }
    }

    internal sealed class AccessVisibilityChecker
    {
        private static readonly Dictionary<AccessRights, Func<AccessRights, bool>> _accessRightsCompatibility = FillRules();

        private readonly MemberWithAccess _referencedMember;
        private readonly AccessRights _referencedMemberVisibility;

        private AccessVisibilityChecker(AccessRights referencedMemberVisibility)
        {
            Contract.Requires(referencedMemberVisibility != AccessRights.NONE);
            _referencedMemberVisibility = referencedMemberVisibility;
        }

        private AccessVisibilityChecker(MemberWithAccess referencedMember)
        {
            _referencedMember = referencedMember;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_referencedMember != null || _referencedMemberVisibility != AccessRights.NONE);
        }

        [Pure]
        public static AccessVisibilityChecker MemberWith(AccessRights enclosingMember)
        {
            Contract.Ensures(Contract.Result<AccessVisibilityChecker>() != null);
            return new AccessVisibilityChecker(enclosingMember);
        }

        [Pure]
        public static AccessVisibilityChecker Member(MemberWithAccess member)
        {
            Contract.Requires(member != null);
            Contract.Ensures(Contract.Result<AccessVisibilityChecker>() != null);
            return new AccessVisibilityChecker(member);
        }

        [Pure]
        public bool IsAccessibleFrom(AccessRights contractHolderVisibility)
        {
            if (_accessRightsCompatibility.ContainsKey(contractHolderVisibility))
            {
                return _accessRightsCompatibility[contractHolderVisibility](_referencedMemberVisibility);
            }

            Contract.Assert(false, string.Format("Unknown referencedMember visibility: {0}", contractHolderVisibility));
            return false;
        }

        [Pure]
        public bool IsAccessibleFrom(MemberWithAccess contractHolder)
        {
            Contract.Requires(contractHolder != null);
            Contract.Assert(_referencedMember != null);

            // If type visibilities are the same, lets ignore them!
            if (_referencedMember.TypeAccessRights == contractHolder.TypeAccessRights)
            {
                return MemberWith(_referencedMember.MemberAccessRights)
                        .IsAccessibleFrom(contractHolder.MemberAccessRights);
            }

            var referencedMemberCombinedVisibility = _referencedMember.GetCombinedAccessRights();
            var contractHolderCombinedVisibility = contractHolder.GetCombinedAccessRights();

            return MemberWith(referencedMemberCombinedVisibility).IsAccessibleFrom(contractHolderCombinedVisibility);
        }

        [Pure]
        private static Dictionary<AccessRights, Func<AccessRights, bool>> FillRules()
        {
            // Key is a referencedMember and the value is a predicate that will return true
            // if specified referencedMember is accessible for the key
            var accessRights = new Dictionary<AccessRights, Func<AccessRights, bool>>();
            accessRights[AccessRights.PUBLIC] = ar => ar == AccessRights.PUBLIC;

            accessRights[AccessRights.PROTECTED] = 
                accessRights[AccessRights.PUBLIC]
                    .Or(ar => ar == AccessRights.PROTECTED_OR_INTERNAL)
                    .Or(ar => ar == AccessRights.PROTECTED);

            accessRights[AccessRights.INTERNAL] = 
                accessRights[AccessRights.PUBLIC]
                    .Or(ar => ar == AccessRights.PROTECTED_OR_INTERNAL)
                    .Or(ar => ar == AccessRights.INTERNAL);

            accessRights[AccessRights.PROTECTED_OR_INTERNAL] =
                accessRights[AccessRights.PUBLIC]
                .Or(ar => ar == AccessRights.PROTECTED_OR_INTERNAL);    

            accessRights[AccessRights.PROTECTED_AND_INTERNAL] = 
                accessRights[AccessRights.PUBLIC]
                    .Or(ar => ar == AccessRights.PROTECTED_AND_INTERNAL)
                    .Or(accessRights[AccessRights.PROTECTED])
                    .Or(accessRights[AccessRights.INTERNAL]);
            ;

            accessRights[AccessRights.PRIVATE] = ar => true;
            accessRights[AccessRights.NONE] = ar => false;

            return accessRights;
        }

        public static AccessRights CombineTypeAndMemberAccessRights(AccessRights typeAccess, AccessRights memberAccess)
        {
            if (typeAccess == AccessRights.PUBLIC)
                return memberAccess;

            if (memberAccess == AccessRights.PUBLIC)
                return typeAccess;

            if (typeAccess == AccessRights.PRIVATE || memberAccess == AccessRights.PRIVATE)
                return AccessRights.PRIVATE;

            if (typeAccess == AccessRights.NONE || memberAccess == AccessRights.NONE)
                return AccessRights.NONE;

            if (typeAccess == memberAccess)
                return typeAccess;

            if (typeAccess == AccessRights.INTERNAL)
            {
                switch (memberAccess)
                {
                    case AccessRights.PROTECTED_OR_INTERNAL:
                        return AccessRights.INTERNAL;

                    case AccessRights.PROTECTED:
                    case AccessRights.PROTECTED_AND_INTERNAL:
                        return AccessRights.PROTECTED_AND_INTERNAL;
                    default:
                        Debug.Assert(false, "Illegal case!");
                        return AccessRights.NONE;
                }
            }

            // Those typeAccess are valid only for inner types!
            if (typeAccess == AccessRights.PROTECTED)
            {
                switch (memberAccess)
                {
                    case AccessRights.PROTECTED_OR_INTERNAL:
                        return AccessRights.PROTECTED;

                    case AccessRights.INTERNAL:
                    case AccessRights.PROTECTED_AND_INTERNAL:
                        return AccessRights.PROTECTED_AND_INTERNAL;
                    default:
                        Debug.Assert(false, "Illegal case!");
                        return memberAccess;
                }
            }

            if (typeAccess == AccessRights.PROTECTED_OR_INTERNAL)
            {
                switch(memberAccess)
                {
                    case AccessRights.INTERNAL:
                        return AccessRights.INTERNAL;

                    case AccessRights.PROTECTED:
                        return AccessRights.PROTECTED;
                    
                    case AccessRights.PROTECTED_AND_INTERNAL:
                        return AccessRights.PROTECTED_AND_INTERNAL;
                    default:
                        Debug.Assert(false, "Illegal case!");
                        return memberAccess;
                }
            }

            Debug.Assert(false, "Illegal case!");
            return memberAccess;
        }
    }

    internal class Foo
    {
        // protected & internal
        protected void Method1()
        { }

        // protected & internal
        protected internal void Method2()
        { }
    }

    public class Foo2
    {
        protected internal class Inner
        {
            
        }
        // protected
        protected void Method1()
        { }
        // protected or internal
        protected internal void Method2()
        { }
    }


}