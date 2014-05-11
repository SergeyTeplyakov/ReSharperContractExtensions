using System.Diagnostics.Contracts;
using System;

abstract class A
{
  public string EnabledOnReferenceProperty{on} {get; private set;}

  [ContractInvariantMethod]
  private void ObjectInvariant()
  {
  }

}