using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharper.ContractExtensions.ContextActions.Requires;

namespace ReSharper.ContractExtensions.ContextActions.EnumChecks
{
    [ContextAction(Name = Name, Group = "Contracts", Description = Description, Priority = 100)]
    public class EnumCheckRequiresContextAction : RequiresContextActionBase
    {
        private const string Name = "Check enum for validity";
        private const string Description = "Check that enum is valid by Enum.IsDefined.";

        private EnumCheckRequiresAvailability _availability = EnumCheckRequiresAvailability.Unavailable;

        public EnumCheckRequiresContextAction(ICSharpContextActionDataProvider provider) : base(provider)
        {}

        protected override void ExecuteTransaction()
        {
            var executor = new EnumCheckRequiresExecutor(_availability, false);
            executor.ExecuteTransaction();
        }

        protected override bool DoIsAvailable()
        {
            _availability = EnumCheckRequiresAvailability.CheckIsAvailable(_provider);
            return _availability.IsAvailable;
        }

        protected override string GetTextBase()
        {
            return "Check enum argument with Enum.IsDefined";
        }

        protected override RequiresContextActionBase CreateContextAction()
        {
            return new EnumCheckRequiresContextAction(_provider);
        }
    }
}