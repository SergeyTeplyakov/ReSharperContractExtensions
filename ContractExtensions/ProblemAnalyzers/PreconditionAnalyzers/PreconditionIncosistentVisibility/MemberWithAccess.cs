using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl.UnresolvedSupport;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Impl.Special;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util.DataStructures;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    public enum MemberType
    {
        Method,
        Property,
        Field,
        Event,
        Member
    }

    public sealed class MemberWithAccess
    {
        private readonly IClrDeclaredElement _declaredElement;
        private readonly AccessRights _memberAccessRights;

        internal MemberWithAccess(IClrDeclaredElement declaredElement, AccessRights typeAccessRights,
            MemberType memberType, AccessRights memberAccessRights)
        {
            Contract.Requires(declaredElement != null);

            _declaredElement = declaredElement;

            MemberName = declaredElement.ShortName;
            TypeAccessRights = typeAccessRights;
            MemberType = memberType;
            _memberAccessRights = memberAccessRights;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(MemberName != null);
            Contract.Invariant(MemberTypeString != null);
        }

        [CanBeNull]
        public static MemberWithAccess FromDeclaredElement(IDeclaredElement declaredElement)
        {
            Contract.Requires(declaredElement != null);

            var clrDeclaredElement = declaredElement as IClrDeclaredElement;
            if (clrDeclaredElement == null)
                return null;

            var accessRightsOwner = declaredElement as IAccessRightsOwner;
            if (accessRightsOwner == null)
                return null;


            var declaringTypeAccessRights = declaredElement.GetDeclarations()
                .FirstOrDefault()
                .With(x => x.GetContainingNode<IClassLikeDeclaration>())
                .With(x => (AccessRights?)x.GetAccessRights());

            if (declaringTypeAccessRights == null)
                return null;

            return new MemberWithAccess(clrDeclaredElement, declaringTypeAccessRights.Value,
                GetMemberType(declaredElement), 
                accessRightsOwner.GetAccessRights());
        }

        public string TypeAndMemberName
        {
            get
            {
                return string.Format("{0}.{1}", MemberTypeName, MemberName);
            }
        }

        public string MemberTypeName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _declaredElement.GetContainingType()
                    .With(x => x.GetClrName())
                    .With(x => x.ShortName);
            }
        }
        public string MemberName { get; private set; }

        public AccessRights TypeAccessRights { get; private set; }

        public AccessRights MemberAccessRights
        {
            get
            {
                // TODO: looks like a hack, but this is the easierst solution for now!!
                // Events are actually less visible than they apprears!
                if (MemberType == MemberType.Event)
                    return AccessRights.PRIVATE;

                return _memberAccessRights;
            }
        }

        public MemberType MemberType { get; private set; }

        public string MemberTypeString
        {
            get
            {
                switch (MemberType)
                {
                    case MemberType.Method:
                        return "method";
                    case MemberType.Property:
                        return "property";
                    case MemberType.Field:
                        return "field";
                    default:
                        return "member";
                }
            }
        }

        public IClrDeclaredElement DeclaredElement
        {
            get
            {
                Contract.Ensures(Contract.Result<IClrDeclaredElement>() != null);
                return _declaredElement;
            }
        }

        private static MemberType GetMemberType(IDeclaredElement element)
        {
            Contract.Requires(element != null);

            if (element is IProperty)
                return MemberType.Property;
            if (element is IMethod)
                return MemberType.Method;
            if (element is IField)
                return MemberType.Field;
            if (element is IEvent)
                return MemberType.Event;

            return MemberType.Member;
        }
    }

    public static class MemberWithAccessEx
    {
        public static AccessRights GetCombinedAccessRights(this MemberWithAccess member)
        {
            Contract.Requires(member != null);

            return AccessVisibilityChecker.CombineTypeAndMemberAccessRights(member.TypeAccessRights,
                member.MemberAccessRights);
        }

        public static bool BelongToTheSameType(this MemberWithAccess preconditionContainer, MemberWithAccess referencedMember)
        {
            Contract.Requires(preconditionContainer != null);
            Contract.Requires(referencedMember != null);

            var preconditionContainingType =
                preconditionContainer.DeclaredElement.GetContainingType();

            var enclosingMemberContiningType =
                referencedMember.DeclaredElement.GetContainingType();

            if (preconditionContainingType == null || enclosingMemberContiningType == null)
                return false;

            return preconditionContainingType.GetClrName().FullName ==
                   enclosingMemberContiningType.GetClrName().FullName;
        }

        public static bool BelongToTheSameProject(this MemberWithAccess preconditionContainer, MemberWithAccess referencedMember)
        {
            Contract.Requires(preconditionContainer != null);
            Contract.Requires(referencedMember != null);

            var containerProject =
                preconditionContainer.DeclaredElement.GetContainingType()
                    .With(x => x.GetSingleOrDefaultSourceFile())
                    .Return(x => x.GetProject());

            var referencedMemberProject =
                referencedMember.DeclaredElement.GetContainingType()
                    .With(x => x.GetSingleOrDefaultSourceFile())
                    .Return(x => x.GetProject());

            if (containerProject == null || referencedMemberProject == null)
                return false;

            return containerProject.Guid == referencedMemberProject.Guid;
        }

    }
}