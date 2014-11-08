using System;
using System.Diagnostics.Contracts;

class Person
{
  public string Name {get; set;}
}

abstract class D3
{  
  public string this[int index]
  {
    s{on}et
    {
        return "42";
    }
  }
}