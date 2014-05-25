using System;
using System.Diagnostics.Contracts;

abstract class A
{
  private string _enabledOnReferenceType{on};
  private int? _enabledOnNullableInt{on};

  private int _disabledOnInt{off};
  private static string _disabledOnStaticFields{off};

  private string _disabledOnStringIfAlreadyChecked{off};

  [ContractInvariantMethod]
  private void ObjectInvariant()
  {
    Contract.Invariant(_disabledOnStringIfAlreadyChecked != null);
    Contract.Invariant(DisabledIfAlreadyChecked != null);
  }

  public string EnabledOnReferenceProperty{on} {get; private set;}
  public int? EnabledOnNullableProperty{on} {get; private set;}

  public int DisabledOnValueTypeProperty{off} {get; private set;}
  public string DisabledOnSettableOnlyProperty {set {}}

  public string DisabledIfAlreadyChecked{off} {get; private set;}

}