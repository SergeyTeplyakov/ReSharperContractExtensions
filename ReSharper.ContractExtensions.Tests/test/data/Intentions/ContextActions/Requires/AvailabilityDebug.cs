using System;
using System.Diagnostics.Contracts;

class Person
{
  public string Name {get; set;}
}


abstract class A
{  
  public void DisabledOnAlreadyCheckByIfThrow(string s{off})
  {
    if (s == null)
     throw new System.ArgumentNullException("s");
  }
}