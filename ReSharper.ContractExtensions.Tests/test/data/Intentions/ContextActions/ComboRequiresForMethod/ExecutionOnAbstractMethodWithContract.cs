using System.Diagnostics.Contracts;

[ContractClass (typeof(AContract))]
abstract class A
{
  public abstract void EnabledOnAbstractMethodWitContract{caret}(string s, int? n);
}

[ContractClassFor (typeof(A))]
abstract class AContract : A
{
  public override void EnabledOnAbstractMethodWitContract(string s, int? n)
  {
    throw new System.NotImplementedException();
  }
}