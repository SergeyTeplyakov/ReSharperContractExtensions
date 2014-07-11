using System;
using System.Diagnostics.Contracts;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Impl;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [QuickFix]
    public class ContractPublicPropertyNameQuickFix : QuickFixBase
    {
        private readonly ContractPublicPropertyNameHighlighing _highlighing;
        private bool _changeVisibility;
        private bool _generateProperty;

        public ContractPublicPropertyNameQuickFix(ContractPublicPropertyNameHighlighing highlighing)
        {
            Contract.Requires(highlighing != null);

            _highlighing = highlighing;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!_changeVisibility || !_generateProperty, 
                "Only one of those field could be true!");
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            if (_changeVisibility)
            {
                Contract.Assert(_highlighing.ReferingFieldOrProperty != null);
                ModifiersUtil.SetAccessRights(_highlighing.ReferingFieldOrProperty, AccessRights.PUBLIC);
            }

            if (_generateProperty)
            {
                Contract.Assert(_highlighing.ReferingFieldOrProperty == null);
                GenerateRefereingProperty();
            }
            return null;
        }

        public override string Text
        {
            get
            {
                if (_changeVisibility)
                {
                    return string.Format("Change visibility of the property '{0}' to 'public'",
                        _highlighing.PropertyName);

                }

                return string.Format("Generate public property '{0}' with type '{1}'", 
                    _highlighing.PropertyName, _highlighing.FieldType);
            }
        }

        public override bool IsAvailable(IUserDataHolder cache)
        {
            if (_highlighing.ReferingFieldOrProperty == null)
            {
                _generateProperty = true;
                return true;
            }

            // Let's disable quick fix for fields. It sounds crazy to fix visibility of one field instead of another!
            if (_highlighing.ReferingFieldOrProperty is IFieldDeclaration)
                return false;

            _changeVisibility = true;
            return true;
        }

        private void GenerateRefereingProperty()
        {
            var fieldDeclaration = _highlighing.FieldDeclaration;
            var factory = CSharpElementFactory.GetInstance(fieldDeclaration);

            var propertyDeclaration =
                (IClassMemberDeclaration)factory.CreateTypeMemberDeclaration(
                    "public $0 $1 {get {return $2;}}",
                    fieldDeclaration.Type, _highlighing.PropertyName, fieldDeclaration.NameIdentifier);

            var typeDeclaration = (IClassLikeDeclaration)fieldDeclaration.GetContainingTypeDeclaration();

            typeDeclaration.AddClassMemberDeclarationAfter(propertyDeclaration, fieldDeclaration);
        }
    }
}