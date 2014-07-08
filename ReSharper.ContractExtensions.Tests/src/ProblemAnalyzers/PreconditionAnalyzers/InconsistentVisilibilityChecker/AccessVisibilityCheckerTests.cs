using JetBrains.ReSharper.Psi;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers
{
    [TestFixture]
    public class AccessVisibilityCheckerTests
    {
        /// <summary>
        /// Sample: void Foo(string s) {Contract.Requires(!IsNullOrEmpty(s));}
        /// In this case: IsNullOrEmpty is enclosing member and Foo method is precondition holder.
        /// And Code Contracts rules are: Enclosing Member should be as visible as precondition holder
        /// </summary>
        
        // For public methods precondition should have only public members
        [TestCase(AccessRights.PUBLIC, AccessRights.PUBLIC, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.PUBLIC, Result = false)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PUBLIC, Result = false)]
        [TestCase(AccessRights.PROTECTED, AccessRights.PUBLIC, Result = false)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PUBLIC, Result = false)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PUBLIC, Result = false)]

        // For private methods any enclosing is available
        [TestCase(AccessRights.PUBLIC, AccessRights.PRIVATE, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.PRIVATE, Result = true)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PRIVATE, Result = true)]
        [TestCase(AccessRights.PROTECTED, AccessRights.PRIVATE, Result = true)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PRIVATE, Result = true)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PRIVATE, Result = true)]

        // For protected methods
        [TestCase(AccessRights.PUBLIC, AccessRights.PROTECTED, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.PROTECTED, Result = false)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PROTECTED, Result = false)]
        [TestCase(AccessRights.PROTECTED, AccessRights.PROTECTED, Result = true)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PROTECTED, Result = false)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PROTECTED, Result = true)]

        // For internal methods
        [TestCase(AccessRights.PUBLIC, AccessRights.INTERNAL, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.INTERNAL, Result = false)]
        [TestCase(AccessRights.INTERNAL, AccessRights.INTERNAL, Result = true)]
        [TestCase(AccessRights.PROTECTED, AccessRights.INTERNAL, Result = false)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.INTERNAL, Result = false)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.INTERNAL, Result = true)]

        // For protected or internal
        [TestCase(AccessRights.PUBLIC, AccessRights.PROTECTED_OR_INTERNAL, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.PROTECTED_OR_INTERNAL, Result = false)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PROTECTED_OR_INTERNAL, Result = true)]
        [TestCase(AccessRights.PROTECTED, AccessRights.PROTECTED_OR_INTERNAL, Result = true)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PROTECTED_OR_INTERNAL, Result = false)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PROTECTED_OR_INTERNAL, Result = true)]

        // For protected and internal
        [TestCase(AccessRights.PUBLIC, AccessRights.PROTECTED_AND_INTERNAL, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.PROTECTED_AND_INTERNAL, Result = false)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PROTECTED_AND_INTERNAL, Result = false)]
        [TestCase(AccessRights.PROTECTED, AccessRights.PROTECTED_AND_INTERNAL, Result = false)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PROTECTED_AND_INTERNAL, Result = true)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PROTECTED_AND_INTERNAL, Result = false)]

        public bool Test_Visibility(AccessRights enclosingMember, AccessRights preconditionHolder)
        {
            return AccessVisibilityChecker.MemberWith(enclosingMember).IsAccessibleFrom(preconditionHolder);
        }
    }
}