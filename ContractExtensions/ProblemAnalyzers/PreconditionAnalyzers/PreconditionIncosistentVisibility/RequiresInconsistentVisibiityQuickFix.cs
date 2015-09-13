using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Refactorings.Util;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    /// <summary>
    /// Fix inconsistent visibility of the referenced member in the precondition.
    /// </summary>
    [QuickFix]
    public sealed class RequiresInconsistentVisibiityQuickFix : QuickFixBase
    {
        private AccessRights? _destinationTypeAccess;
        private AccessRights? _destinationMemberAccess;

        private readonly RequiresInconsistentVisibiityHighlighting _highlighting;

        public RequiresInconsistentVisibiityQuickFix(RequiresInconsistentVisibiityHighlighting highlighting)
        {
            Contract.Requires(highlighting != null);

            _highlighting = highlighting;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            Contract.Assert(_destinationMemberAccess != null || _destinationTypeAccess != null);

            // TODO: R# documentation saying that GetDeclarations is VERY expensive!
            // Can I do the same but in some other way???

            if (_destinationMemberAccess != null)
                FixReferencedMemberAccess(_destinationMemberAccess.Value);

            if (_destinationTypeAccess != null)
                FixReferencedTypeAccess(_destinationTypeAccess.Value);

            return null;
        }

        private void FixReferencedTypeAccess(AccessRights newTypeAccess)
        {
            var declaration =
                _highlighting.LessVisibleReferencedMember.DeclaredElement
                .With(x => x.GetContainingType())
                .With(x => x.GetDeclarations().FirstOrDefault());

            Contract.Assert(declaration != null);

            ModifiersUtil.SetAccessRights(
                declaration,
                newTypeAccess);
        }

        private void FixReferencedMemberAccess(AccessRights memberAccessRights)
        {
            var declaration =
                _highlighting.LessVisibleReferencedMember.DeclaredElement
                    .GetDeclarations().FirstOrDefault();

            Contract.Assert(declaration != null);

            ModifiersUtil.SetAccessRights(
                declaration,
                memberAccessRights);
        }

        public override string Text
        {
            get
            {
                Contract.Assert(_destinationMemberAccess != null || _destinationTypeAccess != null);

                var referencedMember = _highlighting.LessVisibleReferencedMember;

                if (_destinationTypeAccess == null)
                {
                    return string.Format("Change visibility of the referenced {0} '{1}' to '{2}'",
                        referencedMember.MemberTypeString, referencedMember.MemberName,
                        _destinationMemberAccess.Value.ToCSharpString());
                }

                if (_destinationMemberAccess == null)
                {
                    return string.Format("Change visibility of the referenced type '{0}' to '{1}'",
                        referencedMember.MemberTypeName, _destinationTypeAccess.Value.ToCSharpString());
                }

                return string.Format(
                    "Change visibility of the referenced {0} '{1}' to '{2}' and type '{3}' to '{4}'",
                    referencedMember.MemberTypeString, referencedMember.MemberName,
                    _destinationMemberAccess.Value.ToCSharpString(), referencedMember.MemberTypeName, 
                    _destinationTypeAccess.Value.ToCSharpString());
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return ComputeDestinationTypeAndMemberAccessCore(out _destinationTypeAccess, out _destinationMemberAccess);
        }

        private bool ComputeDestinationTypeAndMemberAccessCore(out AccessRights? typeAccess, out AccessRights? memberAccess)
        {
            typeAccess = null;
            memberAccess = null;

            var preconditionContainer = _highlighting.PreconditionContainer;
            var referencedMember = _highlighting.LessVisibleReferencedMember;

            if (referencedMember.MemberType == MemberType.Event)
            {
                // We can't fix event visibility!
                return false;
            }

            if (preconditionContainer.BelongToTheSameType(referencedMember))
            {
                memberAccess = _highlighting.PreconditionContainer.MemberAccessRights;
                return true;
            }

            // We can't fix visibility for members from the different projects
            // (potentially, it's possible, but I don't care about this case for now)

            if (!preconditionContainer.BelongToTheSameProject(referencedMember))
            {
                return false;
            }

            // Referenced member is defined in another type.
            // Lets find out, is it possible to fix this issue!

            // If contract holder is protected we're really limited in our fixes!
            if (preconditionContainer.MemberAccessRights == AccessRights.PROTECTED)
            {
                // The only way we can fix visibility of the referenced member,
                // when the referenced member defined in the base class!

                var referencedType = (IClass)referencedMember.DeclaredElement.GetContainingType();
                var preconditionsClass = (IClass)preconditionContainer.DeclaredElement.GetContainingType();

                if (preconditionsClass.IsSuperType(referencedType))
                {
                    // Referenced member declared in one of the base types for member with contract!
                    memberAccess = preconditionContainer.MemberAccessRights;
                    return true;
                }

                return false;
            }

            // Simple case: members have the same access, the only one fix required: fix of the type access
            if (preconditionContainer.MemberAccessRights == referencedMember.MemberAccessRights)
            {
                typeAccess = preconditionContainer.TypeAccessRights;
                return true;
            }

            if (preconditionContainer.TypeAccessRights != referencedMember.TypeAccessRights)
            {
                typeAccess = preconditionContainer.TypeAccessRights;
            }

            memberAccess = preconditionContainer.MemberAccessRights;
            
            return true;
        }

        
    }

    
}