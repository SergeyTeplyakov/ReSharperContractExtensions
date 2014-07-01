using System;
using System.Diagnostics.Contracts;


class A
{
  private string _disabledOnStringIfAlreadyChecked{off};

  [ContractInvariantMethod]
  private void ObjectInvariant()
  {
    Contract.Invariant(_disabledOnStringIfAlreadyChecked != null);
  }

}