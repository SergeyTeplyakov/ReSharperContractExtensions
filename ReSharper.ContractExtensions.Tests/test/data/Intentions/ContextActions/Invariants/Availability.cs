using System.Diagnostics.Contracts;
using System;

struct A
{
  public string EnabledOnReferenceProperty{on} {get; private set;}

  [ContractInvariantMethod]
  private void ObjectInvariant()
  {
  }

}