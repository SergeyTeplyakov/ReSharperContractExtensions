using System;
using System.Diagnostics.Contracts;

abstract class A
{  
  public object this[string index]
  {
    g{caret}et
    {
      return new object();
    }
  }
}