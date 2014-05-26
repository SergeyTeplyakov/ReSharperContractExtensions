using System.Diagnostics.Contracts;
using System;

abstract class A
{
  public string{on} EnabledInsideMethod()
  {
      var f = "";
      return "foo";
  }

}