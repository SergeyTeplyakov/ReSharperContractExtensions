using System.Diagnostics.Contracts;

[ContractClass(typeof(AContract))]
abstract class A
{
  public abstract void EnabledOnAbstractMethodWithoutContract(string s{on});

  public abstract void DisabledOnAbstractMethod(string s{off});
  protected abstract string DisabledOnAbstracmMethodWithResult(string s{off});
}

[ContractClassFor(typeof(A))]
abstract class AContract : A
{
  public override void DisabledOnAbstractMethod(string s)
  {
  }

  protected override string DisabledOnAbstracmMethodWithResult(string s)
  {
    throw new System.NotImplementedException();
  }

}