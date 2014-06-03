using System.Diagnostics.Contracts;

[ContractClass (typeof(AContract))]
abstract class A
{
  public void EnableOnSimpleMethod{on}(string s, string s2) {}
  public void EnableOnSimpleMethodWithDefaults{on}(string s, string s2, int n, int? nn, string s3 = null) {}

  public abstract void EnabledOnAbstractMethodWithoutContract{on}(string s, int? n);
  public abstract void EnabledOnAbstractMethodWitContract{on}(string s, int? n);

  public abstract void DisabledBecauseAllChecked{off}(string s, int? n);
  public void DisabledBecauseNoneArgsAvailable{off}(int n, string s = null) {}
  public void DisabledBecauseNoArgument{off}() {}
  public void DisabledBecauseAllArgsChecked{off}(string s, int? n)
  {
    Contract.Requires(s != null);
    Contract.Requires(n != null);
  }

  public string DisabledOnProperty{off} {set {}}
}

[ContractClassFor (typeof(A))]
abstract class AContract : A
{
  public override void DisabledBecauseAllChecked(string s, int? n)
  {
    Contract.Requires(s != null);
    Contract.Requires(n != null);

    throw new System.NotImplementedException();
  }

  public override void EnabledOnAbstractMethodWitContract(string s, int? n)
  {
    throw new System.NotImplementedException();
  }
}