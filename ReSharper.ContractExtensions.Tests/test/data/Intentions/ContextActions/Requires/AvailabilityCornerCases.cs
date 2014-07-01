using System.Diagnostics.Contracts;
using System;

class Person
{
  public string Name {get; set;}
}

abstract class A
{
  public void EnabledIfOnlyPropertyChecked(Person person{on})
  {
    Contract.Requires(person.Name != null);
  }

  public void EnabledIfOnlyPropertyCheckedByMethodCall(Person person{on})
  {
    Contract.Requires(!string.IsNullOrEmpty(person.Name));
  }

  public void EnabledIfOnlyPropertyCheckedByIfThrow(Person person{on})
  {
    if (person.Name == null)
      throw new ArgumentNullException("person");
  }

  public void EnabledOnParamsArguments(params object[] arguments{on})
  {}

  public void DisabledBecauseAlreadyCheckedInFirstComplex(string s1, string s2{off}) 
  {
    Contract.Requires((s1 != null || s1.Length == 0) && s2 != null);
  }

  public void DisableOnAlreadyCheckedByStringNullOrEmpty(string s{off})
  {
    Contract.Requires(!string.IsNullOrEmpty(s));
  }

  public void DisableOnAlreadyCheckedByStringNullOrWhiteSpace(string s{off})
  {
    Contract.Requires(!string.IsNullOrWhiteSpace(s));
  }

  public void DisabledOnAlreadyCheckByIfThrow(string s{off})
  {
    if (s == null)
     throw new System.ArgumentNullException("s");
  }

  public void DisableOnAlreadyCheckByIfThrow2(string s{off})
  {
    if (s == null)
    {
      throw new ArgumentNullException("s", "some message");
    }
  }
}