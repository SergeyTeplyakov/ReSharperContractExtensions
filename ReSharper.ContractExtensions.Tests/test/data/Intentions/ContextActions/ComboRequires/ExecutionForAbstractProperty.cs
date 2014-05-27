using System.Diagnostics.Contracts;

[ContractClass(typeof (AContract))]
abstract class A
{
  public abstract string SomeProperty{caret} { get; }
}

[ContractClassFor(typeof (A))]
abstract class AContract : A
{
}