using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
using JetBrains.Application;
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



/// <summary>
///   Must be in the global namespace.
/// </summary>
[SetUpFixture]
// ReSharper disable CheckNamespace
public class SamplePluginTestEnvironmentAssembly : ReSharperTestEnvironmentAssembly
// ReSharper restore CheckNamespace
{
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
        base.SetUp();
        
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