using System;
using System.Diagnostics.Contracts;

abstract class A
{  
  public object this[string index]
  {
    se{caret}t
    {
      Consonle.WriteLine(42);
    }
  }
}