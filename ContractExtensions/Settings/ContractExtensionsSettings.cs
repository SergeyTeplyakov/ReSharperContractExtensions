using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharper.ContractExtensions.Settings
{
    [SettingsKey(typeof(EnvironmentSettings), "Contract extensions settings")]
    public sealed class ContractExtensionsSettings
    {
        [SettingsEntry(false, "Use Contract.Requires<ArgumentNullException> by default")]
        public bool UseGenericContractRequires { get; set; }

        [SettingsEntry(false, "Check string arguments for null or empty")]
        public bool CheckStringsForNullOrEmpty { get; set; }
    }

    
}