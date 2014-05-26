using System.Diagnostics.Contracts;

[ContractClass(typeof (AContract))]
abstract class A
{
  public abstract string{caret} EnabledOnAbstractMethod();
}

[ContractClassFor(typeof (A))]
abstract class AContract : A
{
}