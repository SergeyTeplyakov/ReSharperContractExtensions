using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JetBrains.Application.Components;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.TextControl;
using JetBrains.Util;

namespace JetBrains.ReSharper.Intentions.CSharp.Test
{
    public abstract class CSharpContextActionAvailabilityTestBase<TContextAction> : CSharpContextActionAvailabilityTestBase where TContextAction : class, IContextAction
    {
        protected override IContextAction CreateContextAction(ICSharpContextActionDataProvider dataProvider)
        {
            return (TContextAction)Activator.CreateInstance(typeof (TContextAction), new object[] {dataProvider});
            //return ShellInstance.GetComponent<TContextAction>();
        }
    }
}