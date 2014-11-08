using System;
using System.Diagnostics.Contracts;

abstract class A
{  
  public object this[string inde{caret}x]
  {
    get
    {
      return new object();
    }
  }
}