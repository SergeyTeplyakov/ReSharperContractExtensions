using System.Diagnostics.Contracts;

[ContractClass(typeof (SomeClassContract))]
abstract class SomeClass{on}
{
  public abstract void MethodWithPrecondition(string s);
}

[ContractClassFor(typeof (SomeClass))]
abstract class SomeClassContract : SomeClass
{
}