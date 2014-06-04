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

            settings.SetBinding(lifetime, UseGenericRequires, UseGenericContractRequires);
            settings.SetBinding(lifetime, CheckForNullOrEmpty, CheckStringsForNullOrEmpty);

            Reset = new DelegateCommand(ResetExecute);
        }

        [NotNull] public IProperty<bool> UseGenericContractRequires { get; private set; }
        [NotNull] public IProperty<bool> CheckStringsForNullOrEmpty { get; private set; }

        [NotNull]
        public ICommand Reset { get; private set; }

        public static readonly Expression<Func<ContractExtensionsSettings, bool>>
            UseGenericRequires = x => x.UseGenericContractRequires,
            CheckForNullOrEmpty = x => x.CheckStringsForNullOrEmpty;

        private void ResetExecute()
        {
            UseGenericContractRequires.SetValue(false);
            CheckStringsForNullOrEmpty.SetValue(false);
        }
    }
}