using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
using JetBrains.Application;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using JetBrains.Threading;
using NUnit.Framework;
using ReSharper.ContractExtensions.ContextActions;
using ReSharper.ContractExtensions.ContextActions.Requires;

[assembly: AssemblyTitle("ReSharper.ContractExtensions.Tests")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ReSharper.ContractExtensions.Tests")]
[assembly: AssemblyCopyright("Copyright ©  2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("f7920b4d-1d2f-47f4-9d69-b6fd7fd32a70")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

#pragma warning disable 618
[assembly: TestDataPathBase(@".\test\data")]
#pragma warning restore 618


[ZoneDefinition]
public class IExtensionTestEnvironmentZone : ITestsZone, IRequire<PsiFeatureTestZone>
{
}

[SetUpFixture]
public class ReSharperTestEnvironmentAssembly : ExtensionTestEnvironmentAssembly<IExtensionTestEnvironmentZone>
{
    //[NotNull]
    //private static IEnumerable<Assembly> GetAssembliesToLoad()
    //{
    //  yield return typeof(PostfixTemplatesManager<>).Assembly;
    //  yield return Assembly.GetExecutingAssembly();
    //}
    //
    //public override void SetUp()
    //{
    //  base.SetUp();
    //
    //  ReentrancyGuard.Current.Execute("LoadAssemblies", () => {
    //    var assemblyManager = Shell.Instance.GetComponent<AssemblyManager>();
    //    assemblyManager.LoadAssemblies(GetType().Name, GetAssembliesToLoad());
    //  });
    //}
    //
    //public override void TearDown()
    //{
    //  ReentrancyGuard.Current.Execute("UnloadAssemblies", () => {
    //    var assemblyManager = Shell.Instance.GetComponent<AssemblyManager>();
    //    assemblyManager.UnloadAssemblies(GetType().Name, GetAssembliesToLoad());
    //  });
    //
    //  base.TearDown();
    //}
}

/// <summary>
///   Must be in the global namespace.
/// </summary>
[SetUpFixture]
// ReSharper disable CheckNamespace
public class SamplePluginTestEnvironmentAssembly : ReSharperTestEnvironmentAssembly
// ReSharper restore CheckNamespace
{
    public SamplePluginTestEnvironmentAssembly()
    {
        Console.WriteLine("Constructor!");
    }

    /// <summary>
    /// Gets the assemblies to load into test environment.
    /// Should include all assemblies which contain components.
    /// </summary>
    private static IEnumerable<Assembly> GetAssembliesToLoad()
    {
        // TestSimpleAvailability assembly
        yield return Assembly.GetExecutingAssembly();

        // Plugin code
        yield return typeof(AddRequiresContextAction).Assembly;
    }

    public override void SetUp()
    {
        try
        {
            base.SetUp();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        ReentrancyGuard.Current.Execute(
          "LoadAssemblies",
          () => Shell.Instance.GetComponent<AssemblyManager>().LoadAssemblies(
            GetType().Name, GetAssembliesToLoad()));
        System.Console.WriteLine("SetUp.End");
    }

    public override void TearDown()
    {
        ReentrancyGuard.Current.Execute(
          "UnloadAssemblies",
          () => Shell.Instance.GetComponent<AssemblyManager>().UnloadAssemblies(
            GetType().Name, GetAssembliesToLoad()));
        base.TearDown();
    }
}