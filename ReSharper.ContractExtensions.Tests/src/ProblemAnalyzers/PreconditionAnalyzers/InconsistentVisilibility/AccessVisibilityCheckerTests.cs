using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.DataStructures;
using NUnit.Framework;
using ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers;

namespace ReSharper.ContractExtensions.Tests.ProblemAnalyzers.PreconditionAnalyzers.InconsistentVisibility
{
    [TestFixture]
    public class AccessVisibilityCheckerTests
    {
        [TestCase(AccessRights.PUBLIC, AccessRights.PUBLIC, Result = AccessRights.PUBLIC)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PUBLIC, Result = AccessRights.INTERNAL)]
        [TestCase(AccessRights.PUBLIC, AccessRights.INTERNAL, Result = AccessRights.INTERNAL)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PRIVATE, Result = AccessRights.PRIVATE)]
        [TestCase(AccessRights.PRIVATE, AccessRights.INTERNAL, Result = AccessRights.PRIVATE)]

        [TestCase(AccessRights.INTERNAL, AccessRights.PROTECTED, Result = AccessRights.PROTECTED_AND_INTERNAL)]
        public AccessRights Test_Combining_Type_And_Member_Access_Rights(AccessRights typeAccess,
            AccessRights memberAccess)
        {
            return AccessVisibilityChecker.CombineTypeAndMemberAccessRights(typeAccess, memberAccess);
        }

        // contract: public/private referenced: private/public -> ok
        // contract: public/internal referenced: internal/public -> ok
        // contract: public/internal referenced: internal/internal -> ok
        [TestCase("PUBLIC/PRIVATE", "PRIVATE/PUBLIC", Result = true)]
        [TestCase("PUBLIC/INTERNAL", "INTERNAL/PUBLIC", Result = true)]
        [TestCase("PUBLIC/INTERNAL", "INTERNAL/INTERNAL", Result = true)]
        public bool Test_Accessible_Visibility(string contractMember, string referencedMember)
        {
            var contractAccesses =
                contractMember
                .Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => (AccessRights)Enum.Parse(typeof (AccessRights), s))
                .ToList();
            var contract = Create(contractAccesses[0], contractAccesses[1]);

            var referencedMemberAccesses =
                referencedMember
                .Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => (AccessRights)Enum.Parse(typeof(AccessRights), s))
                .ToList();
            var member = Create(referencedMemberAccesses[0], referencedMemberAccesses[1]);

            return AccessVisibilityChecker.Member(member).IsAccessibleFrom(contract);
        }

        /// <summary>
        /// Sample: void Foo(string s) {Contract.Requires(!IsNullOrEmpty(s));}
        /// In this case: IsNullOrEmpty is enclosing member and Foo method is precondition holder.
        /// And Code Contracts rules are: Enclosing Member should be as visible as precondition holder
        /// </summary>

        // For public methods precondition should have only public members
        [TestCase(AccessRights.PUBLIC, AccessRights.PUBLIC, Result = true)]
        [TestCase(AccessRights.PUBLIC, AccessRights.PRIVATE, Result = false)]
        [TestCase(AccessRights.PUBLIC, AccessRights.INTERNAL, Result = false)]
        [TestCase(AccessRights.PUBLIC, AccessRights.PROTECTED, Result = false)]
        [TestCase(AccessRights.PUBLIC, AccessRights.PROTECTED_AND_INTERNAL, Result = false)]
        [TestCase(AccessRights.PUBLIC, AccessRights.PROTECTED_OR_INTERNAL, Result = false)]

        // For private methods any enclosing is available
        [TestCase(AccessRights.PRIVATE, AccessRights.PUBLIC, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.PRIVATE, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.INTERNAL, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.PROTECTED, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.PROTECTED_AND_INTERNAL, Result = true)]
        [TestCase(AccessRights.PRIVATE, AccessRights.PROTECTED_OR_INTERNAL, Result = true)]

        // For protected methods
        [TestCase(AccessRights.PROTECTED, AccessRights.PUBLIC, Result = true)]
        [TestCase(AccessRights.PROTECTED, AccessRights.PRIVATE, Result = false)]
        [TestCase(AccessRights.PROTECTED, AccessRights.INTERNAL, Result = false)]
        [TestCase(AccessRights.PROTECTED, AccessRights.PROTECTED, Result = true)]
        [TestCase(AccessRights.PROTECTED, AccessRights.PROTECTED_AND_INTERNAL, Result = false)]
        [TestCase(AccessRights.PROTECTED, AccessRights.PROTECTED_OR_INTERNAL, Result = true)]

        // For internal methods
        [TestCase(AccessRights.INTERNAL, AccessRights.PUBLIC, Result = true)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PRIVATE, Result = false)]
        [TestCase(AccessRights.INTERNAL, AccessRights.INTERNAL, Result = true)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PROTECTED, Result = false)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PROTECTED_AND_INTERNAL, Result = false)]
        [TestCase(AccessRights.INTERNAL, AccessRights.PROTECTED_OR_INTERNAL, Result = true)]

        // For protected or internal
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PUBLIC, Result = true)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PRIVATE, Result = false)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.INTERNAL, Result = false)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PROTECTED, Result = false)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PROTECTED_AND_INTERNAL, Result = false)]
        [TestCase(AccessRights.PROTECTED_OR_INTERNAL, AccessRights.PROTECTED_OR_INTERNAL, Result = true)]

        // For protected and internal
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PUBLIC, Result = true)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PRIVATE, Result = false)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.INTERNAL, Result = true)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PROTECTED, Result = true)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PROTECTED_AND_INTERNAL, Result = true)]
        [TestCase(AccessRights.PROTECTED_AND_INTERNAL, AccessRights.PROTECTED_OR_INTERNAL, Result = true)]

        public bool Test_Visibility(AccessRights contractHolder, AccessRights referencedMember)
        {
            return AccessVisibilityChecker.MemberWith(referencedMember).IsAccessibleFrom(contractHolder);
        }

        internal static MemberWithAccess Create(AccessRights typeAccessRights, AccessRights memberAccessRights)
        {
            return new MemberWithAccess(new Stub(), typeAccessRights, MemberType.Method, memberAccessRights);
        }

        class Stub : IClrDeclaredElement
        {
            public IPsiServices GetPsiServices()
            {
                throw new System.NotImplementedException();
            }

            public IList<IDeclaration> GetDeclarations()
            {
                throw new System.NotImplementedException();
            }

            public IList<IDeclaration> GetDeclarationsIn(IPsiSourceFile sourceFile)
            {
                throw new System.NotImplementedException();
            }

            public DeclaredElementType GetElementType()
            {
                throw new System.NotImplementedException();
            }

            public XmlNode GetXMLDoc(bool inherit)
            {
                throw new System.NotImplementedException();
            }

            public XmlNode GetXMLDescriptionSummary(bool inherit)
            {
                throw new System.NotImplementedException();
            }

            public bool IsValid()
            {
                throw new System.NotImplementedException();
            }

            public bool IsSynthetic()
            {
                throw new System.NotImplementedException();
            }

            public HybridCollection<IPsiSourceFile> GetSourceFiles()
            {
                throw new System.NotImplementedException();
            }

            public bool HasDeclarationsIn(IPsiSourceFile sourceFile)
            {
                throw new System.NotImplementedException();
            }

            public string ShortName { get { return "Foo"; } }
            public bool CaseSensistiveName { get; private set; }
            public PsiLanguageType PresentationLanguage { get; private set; }
            public ITypeElement GetContainingType()
            {
                throw new System.NotImplementedException();
            }

            public ITypeMember GetContainingTypeMember()
            {
                throw new System.NotImplementedException();
            }

            public IPsiModule Module { get; private set; }
            public ISubstitution IdSubstitution { get; private set; }
            public IModuleReferenceResolveContext ResolveContext { get; private set; }
        }
    }
}