using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using ReSharper.ContractExtensions.Utilities;

namespace ReSharper.ContractExtensions.ProblemAnalyzers.PreconditionAnalyzers
{
    [ConfigurableSeverityHighlighting("ContractPublicPropertyNameHighlighing", CSharpLanguage.Name)]
    public sealed class ContractPublicPropertyNameHighlighing : IHighlighting
    {
        private readonly IFieldDeclaration _fieldDeclaration;

        [CanBeNull]private readonly ICSharpTypeMemberDeclaration _referingFieldOrProperty;
        private readonly DocumentRange _range;

        internal ContractPublicPropertyNameHighlighing(IFieldDeclaration fieldDeclaration, 
            string referingFieldOrPropertyName,
            ICSharpTypeMemberDeclaration referingFieldOrProperty)
        {
            Contract.Requires(fieldDeclaration != null);
            Contract.Requires(referingFieldOrPropertyName != null);

            _range = fieldDeclaration.GetHighlightingRange();
            _fieldDeclaration = fieldDeclaration;
            _referingFieldOrProperty = referingFieldOrProperty;

            FieldName = fieldDeclaration.NameIdentifier.With(x => x.Name);
            PropertyName = referingFieldOrPropertyName;
            FieldType = fieldDeclaration.Type.GetLongPresentableName(CSharpLanguage.Instance);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(FieldName != null);
            Contract.Invariant(PropertyName != null);
            Contract.Invariant(FieldType != null);
        }

        public const string ServerityId = "ContractPublicPropertyNameHighlighing";
        // Sample: Field 'ConsoleApplication6.Demo.Analyzers.PreconditionWithContractPublicPropertyName._isValid' 
        // is marked [ContractPublicPropertyName("IsValid")], but no public field/property named 'IsValid' with type 'System.Boolean' can be found
        const string ToolTipWarningFormat = "Field '{0}' is marked [ContractPublicPropertyName(\"{1}\")], but no public field/property named '{1}' with type '{2}' can be found";

        public bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// Calculates range of a highlighting.
        /// </summary>
        public DocumentRange CalculateRange()
        {
            return _range;
        }

        public string ToolTip
        {
            get
            {
                return string.Format(ToolTipWarningFormat, FieldName, PropertyName, FieldType);
            }
        }

        public string FieldName { get; private set; }
        public string PropertyName { get; private set; }
        public string FieldType { get; private set; }

        public string ErrorStripeToolTip { get { return ToolTip; } }
        public int NavigationOffsetPatch { get { return 0; } }

        public IFieldDeclaration FieldDeclaration
        {
            get
            {
                Contract.Ensures(Contract.Result<IFieldDeclaration>() != null);
                return _fieldDeclaration;
            }
        }

        [CanBeNull]
        public ICSharpTypeMemberDeclaration ReferingFieldOrProperty
        {
            get { return _referingFieldOrProperty; }
        }
    }
}