using System;
using JetBrains.Application.Components;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Test;
using JetBrains.TextControl;

namespace JetBrains.ReSharper.Intentions.CSharp.Test
{
    public abstract class CSharpContextActionExecuteTestBase<TContextAction> : CSharpContextActionExecuteTestBase where TContextAction : class, IContextAction
    {
        protected override IContextAction CreateContextAction(ICSharpContextActionDataProvider dataProvider)
        {
            return (TContextAction)Activator.CreateInstance(typeof(TContextAction), new object[] { dataProvider });
        }
    }
}