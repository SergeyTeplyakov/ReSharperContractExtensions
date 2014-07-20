using System.Diagnostics.Contracts;

class A
{
  public void Foo(string s)
  {
    InstanceMethod();
    Contract.Requires(false);
    InstanceMethod();
    StaticMethod();
    Contract.EndContractBlock();
  }
  private void InstanceMethod() {}
  private void StaticMethod() {}
}