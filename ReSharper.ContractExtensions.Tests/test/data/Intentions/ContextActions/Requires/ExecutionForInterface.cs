using System.Diagnostics.Contracts;
using System;

[ContractClass(typeof(IAContract))]
interface IA
{
  void EnabledOnAbstractMethod(string s{caret});
}

[ContractClassFor(typeof(IA))]
abstract class IAContract : IA
{
  public void EnabledOnAbstractMethod(string s)
  {
  }
}