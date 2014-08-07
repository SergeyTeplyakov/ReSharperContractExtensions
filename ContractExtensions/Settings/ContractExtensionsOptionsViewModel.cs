using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Windows.Input;
using JetBrains.Annotations;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store;
using JetBrains.DataFlow;
using JetBrains.UI.Extensions.Commands;
using JetBrains.UI.Options;

namespace ReSharper.ContractExtensions.Settings
{
    public sealed class ContractExtensionsOptionsViewModel
    {
        public ContractExtensionsOptionsViewModel(Lifetime lifetime, OptionsSettingsSmartContext settings)
        {
            Contract.Requires(lifetime != null);
            Contract.Requires(settings != null);

            UseGenericContractRequires = new Property<bool>(lifetime, "UseGenericContractRequires");
            CheckStringsForNullOrEmpty = new Property<bool>(lifetime, "CheckStringsForNullOrEmpty");
            UseExcludeFromCodeCoverageAttribute = new Property<bool>(lifetime, "UseExcludeFromCodeCoverageAttribute");

            settings.SetBinding(lifetime, UseGenericRequiresEx, UseGenericContractRequires);
            settings.SetBinding(lifetime, CheckForNullOrEmptyEx, CheckStringsForNullOrEmpty);
            settings.SetBinding(lifetime, UseExcludeFromCodeCoverageAttributeEx, UseExcludeFromCodeCoverageAttribute);

            Reset = new DelegateCommand(ResetExecute);
        }

        [NotNull] public IProperty<bool> UseGenericContractRequires { get; private set; }
        [NotNull] public IProperty<bool> CheckStringsForNullOrEmpty { get; private set; }
        [NotNull] public IProperty<bool> UseExcludeFromCodeCoverageAttribute { get; private set; }

        [NotNull]
        public ICommand Reset { get; private set; }

        public static readonly Expression<Func<ContractExtensionsSettings, bool>>
            UseGenericRequiresEx = x => x.UseGenericContractRequires,
            CheckForNullOrEmptyEx = x => x.CheckStringsForNullOrEmpty,
            UseExcludeFromCodeCoverageAttributeEx = x => x.UseExcludeFromCodeCoverageAttribute;

        private void ResetExecute()
        {
            UseGenericContractRequires.SetValue(false);
            CheckStringsForNullOrEmpty.SetValue(false);
            UseExcludeFromCodeCoverageAttribute.SetValue(false);
        }
    }
}