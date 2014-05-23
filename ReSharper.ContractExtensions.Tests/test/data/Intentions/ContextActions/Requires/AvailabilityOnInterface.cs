using System.Diagnostics.Contracts;
using System;

[ContractClass(typeof(IAContract))]
interface IA
{
  void EnabledOnAbstractMethod(string s{on});
  void DisableOnNonOverridenMethod(string s{off});
}

[ContractClassFor(typeof(IA))]
abstract class IAContract : IA
{
  public void EnabledOnAbstractMethod(string s)
  {
  }
}