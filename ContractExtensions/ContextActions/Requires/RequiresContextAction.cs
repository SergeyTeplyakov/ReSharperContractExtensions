using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;

namespace ReSharper.ContractExtensions.ContextActions.Requires
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class RequiresContextAction : RequiresContextActionBase
    {
        private const string Name = "Add Contract.Requires";
        private const string Description = "Add Contract.Requires on potentially nullable argument.";

        private const string Format = "Requires '{0}' is not null";

        public RequiresContextAction(ICSharpContextActionDataProvider provider) 
            : base(provider, false)
        {}

        protected override string GetText(string selectedParameterName)
        {
            return string.Format(Format, selectedParameterName);
        }

    }
}