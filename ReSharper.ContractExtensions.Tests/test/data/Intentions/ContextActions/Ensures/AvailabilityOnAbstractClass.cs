using System.Diagnostics.Contracts;
using System;

[ContractClass(typeof(AContract))]
abstract class A
{
  public abstract strin{on}g EnabledOnMethodWithContract();
  
  public abstract string{off} DisabledOnMethodOutsideTheContract();
}

[ContractClassFor(typeof(A))]
abstract class AContract : A
{
  public override string EnabledOnMethodWithContract()
  {
  }
}