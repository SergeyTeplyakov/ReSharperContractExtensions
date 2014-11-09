using System;
using System.Diagnostics.Contracts;

class Person
{
  public string Name {get; set;}
}

abstract class A0
{  
  public object this[string index]
  {
    g{off}et
    {
      return new object();
    }
  }
}
