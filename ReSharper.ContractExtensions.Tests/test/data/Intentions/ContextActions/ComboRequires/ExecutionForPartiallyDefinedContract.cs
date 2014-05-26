using System.Diagnostics.Contracts;

[ContractClass(typeof (AContract))]
abstract class A
{
  public abstract void EnabledOnAbstractMethod(string s{caret});
  public abstract void AnotherMethod(string s);
}

[ContractClassFor(typeof (A))]
abstract class AContract : A
{
}