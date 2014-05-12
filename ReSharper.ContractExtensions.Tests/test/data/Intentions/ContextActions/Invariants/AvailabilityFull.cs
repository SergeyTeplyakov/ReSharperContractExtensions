using System;
using System.Diagnostics.Contracts;

class Person
{
  public string Name {get; set;}
}


class A
{
  private string _enabledOnReferenceType{on};
  private int? _enabledOnNullableInt{on};
  private IComparable _enableOnInterfaceField{on};

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
  public IComparable EnabledOnInterfaceProperty{on} {get; private set;}
  private string EnabledOnPrivateProperty{on} {get; set;}

  public Person EnabledOnPropertyWithUserDefinedType{on} {get; private set;}

  public int DisabledOnValueTypeProperty{off} {get; private set;}
  public string DisabledOnSettableOnlyProperty {set {}}

  public string DisabledIfAlreadyChecked{off} {get; private set;}

}