using System;
using System.Diagnostics.Contracts;

class Person
{
  public string Name {get; set;}
}


abstract class A
{
  protected A(string s{on}) {}

  public void EnabledOnSimpleReferenceArgument(string s{on}) {}
  public void EnabledOnInterfaceArgument(IConvertible c{on}) {}

  void EnabledOnSimpleNullableArgument(int? n{on}) {}

  void EnabledOnStringArgumentWithNotNullDefault(string s{on} = "foo") {}

  void EnabledIfContractRequiresCheckAnotherArgument(string s{on}, string another) 
  {
    Contract.Requires(another != null);
  }

  void EnabledWhenCarretIsInTheBodyOfTheMethod(string s)
  {
    Console.WriteLine(s{on});
  }

  void EnabledWhenCarretIsInTheBodyOfTheMethodOnComplexType(Person person)
  {
    Console.WriteLine(per{on}son.Name);
  }

  void DisabledOnArgumentUsageIfPropertyAccessed(Person person)
  {
    Console.WriteLine(person.Na{off}me);
  }

  void DisableOnOutputParameters(out Person pe{off}rson)
  {
    person = null;
    Console.WriteLine(person.Name);
  }

  
  void EnabledIfContractRequiresExistsButNotChecksNull(string s{on})
  {
    Contract.Requires(s != "");
  }

  void EnabledOnNullableWithNotNullDefault(int? n{on} = 42) {}

  public abstract void DisabledAbstractMethod(string s{off});
  public void DisabledWithValueType(int n{off}) {}
  
  public void DisabledWithReferenceDefaultNull(string s{off} = null) {}
  public void DisabledOnInterfaceWithDefaultNull(IConvertible c{off} = null) {}
  public void DisabledWithNullableDefaultNull(int? n{off} = null) {}

  public void DisabledIfAlreadyCheckedWithArgCheck(string s{on})
  {
    if (s == null) throw new ArgumentNullException("s");
  }

  public void DisabledIfAlreadyCheckedWithRequires(string s{off})
  {
    Contract.Requires(s != null);
  }

  public void DisabledIfAlreadyCheckedWithRequiresAndMessage(string s{off})
  {
    Contract.Requires(s != null, "s != null");
  }


}