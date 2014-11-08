using System;
using System.Diagnostics.Contracts;

abstract class A
{  
  public object this[string inde{caret}x]
  {
    get
    {
      Contract.Requires(index != null);
      return new object();
    }
    set
    {
      Consonle.WriteLine(42);
    }
  }
}