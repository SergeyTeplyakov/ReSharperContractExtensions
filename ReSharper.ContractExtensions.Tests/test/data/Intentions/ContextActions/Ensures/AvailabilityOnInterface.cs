using System.Diagnostics.Contracts;
using System;

[ContractClass(typeof(IAContract))]
interface IA
{
  strin{on}g EnabledOnMethodWithContract();
  string{off} DisabledOnMethodOutsideTheContract();
}

[ContractClassFor(typeof(IA))]
abstract class IAContract : IA
{
  public string EnabledOnMethodWithContract()
  {
    throw new System.NotImplementedException();
  }
}