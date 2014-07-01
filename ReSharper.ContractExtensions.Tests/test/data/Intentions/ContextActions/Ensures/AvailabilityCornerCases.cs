using System.Diagnostics.Contracts;
using System;

abstract class A
{
  public string{off} DisabledBecauseCheckedWithMethodCall()
  {
    Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
    var f = "";
    return "foo";
  }

}