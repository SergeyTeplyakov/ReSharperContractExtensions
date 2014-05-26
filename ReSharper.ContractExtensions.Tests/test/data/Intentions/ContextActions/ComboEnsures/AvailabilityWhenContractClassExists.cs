using System.Diagnostics.Contracts;

[ContractClass(typeof(AContract))]
abstract class A
{
  public abstract string EnabledOnAbstractMethodWithoutContract{on}(string s);

  public abstract string DisabledOnAbstractMethod{off}(string s);
}

[ContractClassFor(typeof(A))]
abstract class AContract : A
{
  public override string DisabledOnAbstractMethod(string s)
  {
    throw new System.NotImplementedException();
  }
}